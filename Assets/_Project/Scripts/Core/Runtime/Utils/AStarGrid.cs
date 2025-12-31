using System;
using System.Collections.Generic;

namespace GridAStar
{
    public readonly struct Point : IEquatable<Point>
    {
        public readonly int x;
        public readonly int y;

        public Point(int x, int y) { this.x = x; this.y = y; }

        public bool Equals(Point other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is Point p && Equals(p);
        public override int GetHashCode() => (x * 73856093) ^ (y * 19349663);
        public override string ToString() => $"({x},{y})";
        public static bool operator ==(Point a, Point b) => a.Equals(b);
        public static bool operator !=(Point a, Point b) => !a.Equals(b);
    }

    internal sealed class Node
    {
        public Point P;
        public int G;
        public int H;
        public int F => G + H;
        public Node Parent;

        public Node(Point p, int g, int h, Node parent)
        {
            P = p; G = g; H = h; Parent = parent;
        }
    }

    //AStar yol bulma algoritmasini uygular
    public sealed class AStarGrid
    {
        private readonly int _w, _h;
        private readonly bool[,] _walkable;

        private static readonly (int dx, int dy)[] _dirs =
        {
            ( 1,  0),
            (-1,  0),
            ( 0,  1),
            ( 0, -1),
        };

        public AStarGrid(int width, int height)
        {
            _w = width; _h = height;
            _walkable = new bool[_w, _h];
            for (int x = 0; x < _w; x++)
                for (int y = 0; y < _h; y++)
                    _walkable[x, y] = true;
        }

        public void SetBlocked(int x, int y, bool blocked = true)
        {
            if (InBounds(x, y)) _walkable[x, y] = !blocked;
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < _w && y < _h;

        public bool IsWalkable(int x, int y) => InBounds(x, y) && _walkable[x, y];

        private static int H(Point a, Point b) => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        //Baslangic noktasindan hedefe yol bulur
        public List<Point> FindPath(Point start, Point goal)
        {
            if (!IsWalkable(start.x, start.y) || !IsWalkable(goal.x, goal.y))
                return null;

            var openList = new List<Node>();
            var openMap  = new Dictionary<Point, Node>();
            var closed   = new HashSet<Point>();

            var startNode = new Node(start, g: 0, h: H(start, goal), parent: null);
            openList.Add(startNode);
            openMap[start] = startNode;

            while (openList.Count > 0)
            {
                var current = PopBest(openList);
                openMap.Remove(current.P);
                closed.Add(current.P);

                if (current.P == goal)
                    return ReconstructPath(current);

                foreach (var (dx, dy) in _dirs)
                {
                    int nx = current.P.x + dx;
                    int ny = current.P.y + dy;
                    var np = new Point(nx, ny);

                    if (!IsWalkable(nx, ny)) continue;
                    if (closed.Contains(np)) continue;

                    int tentativeG = current.G + 1;

                    if (!openMap.TryGetValue(np, out var neighbor))
                    {
                        neighbor = new Node(np, tentativeG, H(np, goal), current);
                        openList.Add(neighbor);
                        openMap[np] = neighbor;
                    }
                    else if (tentativeG < neighbor.G)
                    {
                        neighbor.G = tentativeG;
                        neighbor.Parent = current;
                    }
                }
            }

            return null;
        }

        private static Node PopBest(List<Node> list)
        {
            int bestIdx = 0;
            for (int i = 1; i < list.Count; i++)
            {
                if (Better(list[i], list[bestIdx]))
                    bestIdx = i;
            }
            var best = list[bestIdx];
            list.RemoveAt(bestIdx);
            return best;
        }

        private static bool Better(Node a, Node b)
        {
            if (a.F != b.F) return a.F < b.F;
            if (a.H != b.H) return a.H < b.H;
            return a.G < b.G;
        }

        private static List<Point> ReconstructPath(Node goalNode)
        {
            var path = new List<Point>();
            for (var cur = goalNode; cur != null; cur = cur.Parent)
                path.Add(cur.P);
            path.Reverse();
            return path;
        }
    }
}
