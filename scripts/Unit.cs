using Godot;
using System.Collections.Generic;

public abstract class Unit : Sprite
{
    private const int SPEED = 5;
    private Queue<Vector2> _path = new Queue<Vector2>();
    private int _health = 10;
    private Map _map;
    private AnimationPlayer _animationPlayer;
    private ColorRect _colorRect;
    private Label _label;
    private bool _usedForTurn = false;

    public Vector2 Cell { get; private set; }

    public abstract int MoveRange { get; }
    public abstract int Attack { get; }

    public bool UsedForTurn
    {
        get => _usedForTurn;
        set
        {
            if (value == true)
            {
                SelfModulate = Colors.Gray;
            }
            else
            {
                SelfModulate = Colors.White;
            }
            _usedForTurn = value;
        }
    }

    public void StrobeAnimation()
    {
        _animationPlayer.Play("strobe");
    }

    public void StopAnimation()
    {
        _animationPlayer.Stop();
        SelfModulate = Colors.White;
    }

    public bool TakeDamage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            QueueFree();
            return true;
        }
        _label.Text = _health.ToString();
        _colorRect.Show();
        return false;
    }

    public void Move(List<Vector2> newPath)
    {
        Cell = newPath[newPath.Count - 1];
        foreach (Vector2 newPosition in newPath)
        {
            _path.Enqueue(_map.MapToWorldCenter(newPosition));
        }
        SetProcess(true);
    }

    public override void _Ready()
    {
        _map = GetNode<Map>("/root/World/Map");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _colorRect = GetNode<ColorRect>("ColorRect");
        _label = GetNode<Label>("ColorRect/Label");
        Cell = _map.WorldToMap(Position);
        SetProcess(false);
    }

    public override void _Process(float delta)
    {
        if (_path.Count == 0)
        {
            SetProcess(false);
            return;
        }
        Vector2 movePosition = _path.Peek();
        if (movePosition.x > Position.x)
        {
            if (movePosition.x > Position.x + SPEED)
            {
                Position = new Vector2(Position.x + SPEED, Position.y);
            }
            else
            {
                Position = new Vector2(movePosition.x, Position.y);
            }
        }
        else if (movePosition.x < Position.x)
        {
            if (movePosition.x < Position.x - SPEED)
            {
                Position = new Vector2(Position.x - SPEED, Position.y);
            }
            else
            {
                Position = new Vector2(movePosition.x, Position.y);
            }
        }
        if (movePosition.y > Position.y)
        {
            if (movePosition.y > Position.y + SPEED)
            {
                Position = new Vector2(Position.x, Position.y + SPEED);
            }
            else
            {
                Position = new Vector2(Position.x, movePosition.y);
            }
        }
        else if (movePosition.y < Position.y)
        {
            if (movePosition.y < Position.y - SPEED)
            {
                Position = new Vector2(Position.x, Position.y - SPEED);
            }
            else
            {
                Position = new Vector2(Position.x, movePosition.y);
            }
        }
        if (Position == movePosition)
        {
            _path.Dequeue();
        }
    }

    public override void _ExitTree()
    {
        World world = GetNode<World>("/root/World");
        world.UnitRemoved(this);
    }
}
