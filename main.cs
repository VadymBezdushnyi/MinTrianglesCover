using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System;

//PUT YOUR ALGORITHM CODE HERE: this is the class where you can put your algorithm details. The entry point is Run method but you can change this approach rewriting Solution.Main method below
class MyAlgorithm
{
    private static double INF = 1e10;
    private Random rnd = new Random(41);
    private List<E> ShuffleList<E>(List<E> inputList)
    {
         List<E> randomList = new List<E>();
         int randomIndex = 0;
         while (inputList.Count > 0)
         {
              randomIndex = rnd.Next(0, inputList.Count); //Choose a random object in the list
              randomList.Add(inputList[randomIndex]); //add it to the new, random list
              inputList.RemoveAt(randomIndex); //remove to avoid duplicates
         }
         return randomList; //return the new random list
    }
    
    private bool isTriangleGood(Triangle tr, List<Triangle> selectedTriangles){
        foreach(var trn in selectedTriangles){
            if(tr.IntersectsTriangle(trn)){
                return false;
            }
        }
        return true;
    }
    
    public double getScore(Triangle tr, List<Point> pointsLeft){
        double res = 0; // bigger is better
        foreach (var pt in pointsLeft){
            res += Convert.ToDouble(tr.PointIsInside(pt));
        }
        return res;
    }
    
    public double getScoreSet(List<Triangle> lst, List<Point> pointsLeft){
        double res = 0; // bigger is better
        foreach (var pt in pointsLeft){
            foreach (var tr in lst){
                if(tr.PointIsInside(pt)){
                    res += 1;
                    break;
                }
            }
        }
        return res;
    }
    
    public List<Triangle> generateSortedTriangles(List<Point> points){
        List<Triangle> ans = new List<Triangle>();
        for(int i = 0; i < points.Count; i++){
            for(int j = i+1; j < points.Count; j++){
                for(int k = j+1; k < points.Count; k++){
                    ans.Add(new Triangle(points[i], points[j], points[k]));
                }
            }
        }
        return ans;
    }
    public Triangle selectBestTriangle(List<Triangle> trianglesList, List<Triangle> selectedTriangles, List<Triangle> bestTrianglesList, List<Point> points_left){
        double bestScore = -INF;
        Triangle bestTriangle = trianglesList[0];
        if(rnd.NextDouble() < 0.8 && selectedTriangles.Count == 0 ){
            return bestTrianglesList[rnd.Next(bestTrianglesList.Count)];
        }
        foreach(var candidate in trianglesList){
            if(!selectedTriangles.Any() || isTriangleGood(candidate, selectedTriangles)){
                
                selectedTriangles.Add(candidate);
                double currScore = getScoreSet(selectedTriangles, points_left);
                selectedTriangles.RemoveAt(selectedTriangles.Count - 1);
                
                if(currScore >= bestScore){
                    bestTriangle = candidate;
                    bestScore = currScore;
                }
            }
        }
        return bestTriangle;
    }
    public List<Triangle> generatebestStartTriangles(List<Triangle> allTriangles, List<Point> points){
        List<Triangle> bestTriangles = new List<Triangle>();
        double bestScore = -INF;
        List<double> scores = new List<double>(allTriangles.Count);
        for(int i = 0; i < allTriangles.Count; i++){
            scores.Add(getScore(allTriangles[i], points));
            if(scores[i] > bestScore)
                bestScore = scores[i];
        }
        
        for(int i = 0; i < allTriangles.Count; i++)
            if(scores[i] >= bestScore/2)
                bestTriangles.Add(allTriangles[i]);
        
        return bestTriangles;
    }
    public void Run(Model model)
    {
        var stopwatch = Stopwatch.StartNew();
        
        List<Point> allPoints = model.Points;
        List<Triangle> allTriangles = generateSortedTriangles(allPoints);
        List<Triangle> bestStartTriangles = generatebestStartTriangles(allTriangles, allPoints);
        List<Triangle> selectedTriangles;
            
        double bestScore = INF;

        while(true){
            selectedTriangles = new List<Triangle>();
            while(getScoreSet(selectedTriangles, allPoints) != allPoints.Count){ // while not covered
                Triangle best = selectBestTriangle(allTriangles, selectedTriangles, bestStartTriangles, allPoints);
                selectedTriangles.Add(best);
            }
            
            if(bestScore > selectedTriangles.Count){
                model.Triangles = new List<Triangle>(selectedTriangles);
                bestScore = selectedTriangles.Count;
            }
            
            if(rnd.Next(5) == 0)
                allTriangles = ShuffleList(allTriangles);
            
            if(stopwatch.Elapsed.TotalMilliseconds >= 2900)
                break;
        }
        stopwatch.Stop();
    }
}


//The classes below provide the following functionality:
//* parsing the input data
//* generating the output 
//* simple classes for point, triangle and model for data storage
//* simple math operations like cross product, triangles intersection etc.

// Though these classes help you to proceed to the algorithm immediately, you are 
// not obliged to use any of them. Instead you can clear the existing code and write your own classes.
static class MathUtils
{
    public const double Precision = 0.0001;  
  
    public static bool NearZero(double value)
    {
        return Math.Abs(value) < Precision;
    }  
    
    public static double CrossProd(Point vector1, Point vector2)
    {
        return vector1.X * vector2.Y - vector1.Y * vector2.X;
    }

    public static double DotProd(Point vector1, Point vector2)
    {
        return vector1.X * vector2.X + vector1.Y * vector2.Y;
    }

    public static bool PointsOnSameSideOfLine(Point pt1, Point pt2, Point ptLine1, Point ptLine2, bool bValueToReturnIfOneOfPointsIsOnLine)
    {
        var cross1 = CrossProd(ptLine2 - ptLine1, pt1 - ptLine1);
        var cross2 = CrossProd(ptLine2 - ptLine1, pt2 - ptLine1);

        if (NearZero(cross1) || NearZero(cross2))
            return bValueToReturnIfOneOfPointsIsOnLine;

        return ((cross1 >= 0) && (cross2 >= 0)) ||
                ((cross1 < 0) && (cross2 < 0));
    }

    public static bool PointInTriangle(Point pt, Triangle trn, bool bIfOnBoundaryReturnTrue = true)
    {
           if (NearZero(CrossProd(trn.Points[1] - trn.Points[0], trn.Points[2] - trn.Points[0])))
        {
            return NearZero(CrossProd(trn.Points[1] - pt, trn.Points[2] - pt)) &&            
                (pt.X >= trn.MinX()) && (pt.X <= trn.MaxX()) &&
                (pt.Y >= trn.MinY()) && (pt.Y <= trn.MaxY());
        }
      
        return PointsOnSameSideOfLine(pt, trn.Points[0], trn.Points[1], trn.Points[2], bIfOnBoundaryReturnTrue) &&
                PointsOnSameSideOfLine(pt, trn.Points[1], trn.Points[0], trn.Points[2], bIfOnBoundaryReturnTrue) &&
                PointsOnSameSideOfLine(pt, trn.Points[2], trn.Points[0], trn.Points[1], bIfOnBoundaryReturnTrue);
    }

    public static bool LineSegmentIntersectsLineSegment(Point pt1LineSegm1, Point pt2LineSegm1,
                                                    Point pt1LineSegm2, Point pt2LineSegm2)
    {
        return !(PointsOnSameSideOfLine(pt1LineSegm1, pt2LineSegm1, pt1LineSegm2, pt2LineSegm2, true) ||
                PointsOnSameSideOfLine(pt1LineSegm2, pt2LineSegm2, pt1LineSegm1, pt2LineSegm1, true));
    }

    public static bool TriangleIntersectsTriangle(Triangle trn1, Triangle trn2)
    {
        //Check if triangles' sides intersect
        for (var nSide1 = 0; nSide1 < 3; nSide1++)
        for (var nSide2 = 0; nSide2 < 3; nSide2++)
        {
            if (LineSegmentIntersectsLineSegment(trn1.Points[nSide1], trn1.Points[nSide1 < 2 ? nSide1 + 1 : 0],
                trn2.Points[nSide2], trn2.Points[nSide2 < 2 ? nSide2 + 1 : 0])) return true;
        }

        //Check if the first triangle is completely in the second one
        if (PointInTriangle(trn1.Points[0], trn2, false)) return true;

        //Check if the second triangle is completely in the first one
        if (PointInTriangle(trn2.Points[0], trn1, false)) return true;

        return false;
    }

}

class Point
{
    public List<Triangle> Triangles;

    public double X { get; }

    public double Y { get; }

    public double len(){
        return Math.Sqrt(MathUtils.DotProd(this, this));
    }
    public int Index { set; get; } = -1;

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }
    public bool equal(Point o){
        return X == o.X && Y == o.Y;
    }

    public static Point operator - (Point pt1, Point pt2)
    {
        return new Point(pt2.X - pt1.X, pt2.Y - pt1.Y);
    }

    public static Point operator +(Point pt1, Point pt2)
    {
        return new Point(pt2.X + pt1.X, pt2.Y + pt1.Y);
    }

    public bool InsideTriangle(Triangle trn)
    {
        return MathUtils.PointInTriangle(this, trn);
    }

    public bool ConnectToTriangle(Triangle triangle)
    {
        if (Triangles == null)
            Triangles = new List<Triangle>();

        foreach (var trn in Triangles)
           if (object.ReferenceEquals(triangle, trn)) return false;

        Triangles.Add(triangle);
        return true;
    }
}
class Triangle
{
    public Point[] Points;

    public void ConnectToPoints()
    {
        //Add references to this triangle for all points
        foreach (var point in Points)
            point.ConnectToTriangle(this);
    }
    public bool isEqual(Triangle o){
        return (Points[0] == o.Points[0] && Points[1] == o.Points[1] && Points[2] == o.Points[2]);
    }
    public Triangle(Point pt1, Point pt2, Point pt3)
    {
        Points = new [] {pt1, pt2, pt3};
    }

    public bool PointIsInside(Point pt)
    {
        return MathUtils.PointInTriangle(pt, this);
    }

    public bool IntersectsTriangle(Triangle trn)
    {
        return MathUtils.TriangleIntersectsTriangle(trn, this);
    }
    public double ecc(){
        return radIn() / radOut();
    }
    public double radIn(){
        return Area()/ (.5*Perimetr());
    }
    public double radOut(){
        return sideProd() / (4*Area()); 
    }
    public double sideProd(){
        return (Points[1]-Points[0]).len() *
               (Points[2]-Points[1]).len() *
               (Points[2]-Points[0]).len();
    }
    public double Perimetr(){
        return (Points[1]-Points[0]).len() + (Points[2]-Points[1]).len() + (Points[2]-Points[0]).len();
    }
    public double Area(){
        return Math.Abs(MathUtils.CrossProd(Points[1]-Points[0], Points[2]-Points[0]))/2;
    }
    public double MinX()
    {
        return Math.Min(Points[0].X, Math.Min(Points[1].X, Points[2].X));
    }

    public double MaxX()
    {
        return Math.Max(Points[0].X, Math.Max(Points[1].X, Points[2].X));
    }

    public double MinY()
    {
        return Math.Min(Points[0].Y, Math.Min(Points[1].Y, Points[2].Y));
    }

    public double MaxY()
    {
        return Math.Max(Points[0].Y, Math.Max(Points[1].Y, Points[2].Y));
    }  
}

class Model
{
    public List<Point> Points = new List<Point>();
    public List<Triangle> Triangles = new List<Triangle>();
        
    public Triangle AddTriangle(Point pt1, Point pt2, Point pt3)
    {
        Triangle trn = new Triangle (pt1, pt2, pt3);

        Triangles.Add(trn);

        return trn;
    }

    public bool CheckIfAllTrianglesNotIntersect(out string errorMessage)
    {
        for (var nTriangle = 0; nTriangle < Triangles.Count; nTriangle++)
            for (var nTriangleIntersect = 0; nTriangleIntersect < Triangles.Count; nTriangleIntersect++)
            {
                if ((nTriangle != nTriangleIntersect) &&
                    MathUtils.TriangleIntersectsTriangle(Triangles[nTriangle], Triangles[nTriangleIntersect]))
                {
                    errorMessage =
                        $"Traingle #{nTriangle} intersects with triangle #{nTriangleIntersect}";
                    return false;
                }
            }


        errorMessage = "";
        return true;
    }
}

class DataParser
{
    private readonly Model _model;

    public DataParser(Model model)
    {
        _model = model;
    }

    public void ParseSource()
    {
        string line;
        while ((line = Console.ReadLine()) != null)
        {
            if (line == string.Empty) return;

            var data = line.Trim().Split(' ');
            switch (data.Length)
            {
                case 2:
                    ParseDataForPoint(data);
                    break;
            }
        }
    }

    public void GenerateOutput()
    {
        //Generate triangles list with indexes of points
        foreach (var trn in _model.Triangles)
           Console.WriteLine("{0} {1} {2}", trn.Points[0].Index, trn.Points[1].Index, trn.Points[2].Index);
    }

    private void ParseDataForPoint(string[] data)
    {
        var pt = new Point(double.Parse(data[0]), double.Parse(data[1])) { Index = _model.Points.Count() };
        _model.Points.Add(pt);
    }
}

class Solution {

    static void Main(string[] args) {
        //Create model for keeping all data
        var model = new Model();

        //Fill model with data by parsing data source
        var parser = new DataParser(model);
        parser.ParseSource();

        //Run algorithm for the data model
        new MyAlgorithm().Run(model);

        //Put the result to output
        parser.GenerateOutput();
    }
}