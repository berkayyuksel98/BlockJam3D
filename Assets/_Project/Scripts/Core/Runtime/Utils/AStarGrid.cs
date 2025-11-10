using System;
using System.Collections.Generic;

namespace GridAStar
{
    // Basit 2D nokta tipi
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

    // A* için düğüm
    internal sealed class Node
    {
        public Point P;
        public int G;   // start -> bu node gerçek maliyet (adım sayısı)
        public int H;   // bu node -> hedef Manhattan tahmini
        public int F => G + H;
        public Node Parent;

        public Node(Point p, int g, int h, Node parent)
        {
            P = p; G = g; H = h; Parent = parent;
        }
    }

    public sealed class AStarGrid
    {
        private readonly int _w, _h;
        private readonly bool[,] _walkable; // true = geçilebilir

        // 4 yön (sağ, sol, yukarı, aşağı)
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

        // Manhattan heuristic (4-yön için uygun ve admissible)
        private static int H(Point a, Point b) => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        // start -> goal yolu (liste) döner; yoksa null
        public List<Point> FindPath(Point start, Point goal)
        {
            if (!IsWalkable(start.x, start.y) || !IsWalkable(goal.x, goal.y))
                return null;

            var openList = new List<Node>();                 // işlenecekler
            var openMap  = new Dictionary<Point, Node>();    // hızlı lookup
            var closed   = new HashSet<Point>();             // tamamlananlar

            var startNode = new Node(start, g: 0, h: H(start, goal), parent: null);
            openList.Add(startNode);
            openMap[start] = startNode;

            while (openList.Count > 0)
            {
                // 1) En iyi node'u çek
                var current = PopBest(openList);
                openMap.Remove(current.P);
                closed.Add(current.P);

                // 2) Hedefe ulaşıldı
                if (current.P == goal)
                    return ReconstructPath(current);

                // 3) Komşular
                foreach (var (dx, dy) in _dirs)
                {
                    int nx = current.P.x + dx;
                    int ny = current.P.y + dy;
                    var np = new Point(nx, ny);

                    if (!IsWalkable(nx, ny)) continue;   // engel/taşma
                    if (closed.Contains(np)) continue;    // zaten işlendi

                    int tentativeG = current.G + 1;       // sabit adım maliyeti

                    if (!openMap.TryGetValue(np, out var neighbor))
                    {
                        // yeni keşif
                        neighbor = new Node(np, tentativeG, H(np, goal), current);
                        openList.Add(neighbor);
                        openMap[np] = neighbor;
                    }
                    else if (tentativeG < neighbor.G)
                    {
                        // daha iyi yol bulundu
                        neighbor.G = tentativeG;
                        neighbor.Parent = current;
                        // openList içinde yerinde kalır (küçük listelerde yeterli)
                    }
                }
            }

            // Yol yok
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
            if (a.F != b.F) return a.F < b.F;   // düşük F daha iyi
            if (a.H != b.H) return a.H < b.H;   // sonra düşük H
            return a.G < b.G;                   // sonra düşük G
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
