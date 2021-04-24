using Godot;
using System;
using System.Collections.Generic;

static class Pathfinding
{
    private class PriorityQueue
    {
        private readonly struct PriorityNode
        {
            public readonly Vector2 Cell;
            public readonly int Cost;
            public PriorityNode(Vector2 cell, int cost)
            {
                Cell = cell;
                Cost = cost;
            }
        }
        private readonly LinkedList<PriorityNode> _list = new LinkedList<PriorityNode>();

        public void Push(Vector2 cell, int cost)
        {
            LinkedListNode<PriorityNode> node = _list.First;
            if (node == null || node.Value.Cost > cost)
            {
                _list.AddFirst(new PriorityNode(cell, cost));
            }
            else
            {
                while (node.Next != null && node.Next.Value.Cost <= cost)
                {
                    node = node.Next;
                }
                _list.AddAfter(node, new PriorityNode(cell, cost));
            }
        }

        public Vector2 Pop()
        {
            Vector2 cell = _list.First.Value.Cell;
            _list.RemoveFirst();
            return cell;
        }

        public bool IsEmpty()
        {
            return _list.Count == 0;
        }
    }

    public static HashSet<Vector2> GetMovableCells(Unit unit, Map map, Dictionary<Vector2, Unit> enemyUnits)
    {
        PriorityQueue frontier = new PriorityQueue();
        Dictionary<Vector2, int> costSoFar = new Dictionary<Vector2, int>();
        Vector2 cell = unit.Cell;
        int range = unit.MoveRange;

        frontier.Push(cell, 0);
        costSoFar.Add(cell, 0);

        while (!frontier.IsEmpty())
        {
            cell = frontier.Pop();
            int curCost = costSoFar[cell];
            List<Vector2> neighbors = GetNeighbors(cell, map, enemyUnits);
            foreach (Vector2 neighbor in neighbors)
            {
                int newCost = curCost + map.GetCellCost(neighbor);
                if (newCost <= range && !costSoFar.ContainsKey(neighbor))
                {
                    costSoFar.Add(neighbor, newCost);
                    frontier.Push(neighbor, newCost);
                }
            }
        }

        return new HashSet<Vector2>(costSoFar.Keys);
    }

    public static List<Vector2> GetNewPath(Vector2 start, Vector2 goal, Map map, Dictionary<Vector2, Unit> enemyUnits)
    {
        PriorityQueue frontier = new PriorityQueue();
        Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();
        Dictionary<Vector2, int> costSoFar = new Dictionary<Vector2, int>();

        frontier.Push(start, 0);
        cameFrom.Add(start, new Vector2(9001, 9001));
        costSoFar.Add(start, 0);

        while (!frontier.IsEmpty())
        {
            Vector2 cell = frontier.Pop();
            if (cell == goal)
            {
                break;
            }
            int curCost = costSoFar[cell];
            List<Vector2> neighbors = GetNeighbors(cell, map, enemyUnits);
            foreach (Vector2 neighbor in neighbors)
            {
                int newCost = curCost + map.GetCellCost(neighbor);
                if (!costSoFar.ContainsKey(neighbor))
                {
                    costSoFar.Add(neighbor, newCost);
                    frontier.Push(neighbor, newCost + Heuristic(goal, neighbor));
                    cameFrom.Add(neighbor, cell);
                }
            }
        }

        List<Vector2> path = new List<Vector2>();
        path.Add(goal);
        while (path[path.Count - 1] != start)
        {
            path.Add(cameFrom[path[path.Count - 1]]);
        }
        path.Reverse();
        return path;
    }

    private static List<Vector2> GetNeighbors(Vector2 cell, Map map, Dictionary<Vector2, Unit> enemyUnits)
    {
        Vector2[] DIRECTIONS = {Vector2.Left, Vector2.Right, Vector2.Up, Vector2.Down};
        List<Vector2> neighbors = new List<Vector2>();
        foreach (Vector2 direction in DIRECTIONS)
        {
            Vector2 neighbor = cell + direction;
            if (!enemyUnits.ContainsKey(neighbor) && map.IsInsideMap(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    private static int Heuristic(Vector2 goal, Vector2 cell)
    {
        int goalX = (int)goal.x;
        int goalY = (int)goal.y;
        int cellX = (int)cell.x;
        int cellY = (int)cell.y;
        return Math.Abs(goalX - cellX) + Math.Abs(goalY - cellY);
    }
}
