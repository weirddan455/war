using Godot;
using System.Collections.Generic;

public class World : Node2D
{
    private enum Command {FIRE, WAIT}
    private enum InputType {NOP, SELECT_UNIT, SELECT_ENEMY, DRAW_PATH, CANCEL, MOVE_UNIT}
    private readonly Vector2[] DIRECTIONS = {Vector2.Left, Vector2.Right, Vector2.Up, Vector2.Down};

    private readonly Dictionary<Vector2, Unit> _playerUnits = new Dictionary<Vector2, Unit>();
    private readonly Dictionary<Vector2, Unit> _enemyUnits = new Dictionary<Vector2, Unit>();
    private readonly Dictionary<Vector2, Unit> _enemyNeighbors = new Dictionary<Vector2, Unit>();
    private Godot.Collections.Array _path = new Godot.Collections.Array();
    private Godot.Collections.Dictionary _movableCells = null;
    private Unit _selectedUnit = null;

    private Reference _pathfinding;
    private Map _map;
    private PathOverlay _pathOverlay;
    private PathArrow _pathArrow;
    private VBoxContainer _commandDialog;
    private Button _fireButton;

    public override void _Ready()
    {
        NativeScript nativeScript = (NativeScript)GD.Load("res://gdnative/pathfinding.gdns");
        _pathfinding = (Reference)nativeScript.New();
        _map = GetNode<Map>("Map");
        _pathOverlay = GetNode<PathOverlay>("PathOverlay");
        _pathArrow = GetNode<PathArrow>("PathArrow");
        _commandDialog = GetNode<VBoxContainer>("UI/CommandDialog");
        _fireButton = GetNode<Button>("UI/CommandDialog/Fire");
        foreach (Unit unit in GetNode("PlayerUnits").GetChildren())
        {
            _playerUnits.Add(unit.Cell, unit);
        }
        foreach (Unit unit in GetNode("AIUnits").GetChildren())
        {
            _enemyUnits.Add(unit.Cell, unit);
        }
    }

    private void Combat(Unit attacker, Unit defender)
    {
        defender.TakeDamage(attacker.Attack);
        attacker.TakeDamage(defender.Attack / 2);
    }

    private void GetEnemyNeighbors()
    {
        _enemyNeighbors.Clear();
        foreach (Vector2 direction in DIRECTIONS)
        {
            Vector2 neighbor = _selectedUnit.Cell + direction;
            if (_enemyUnits.ContainsKey(neighbor))
            {
                _enemyNeighbors.Add(neighbor, _enemyUnits[neighbor]);
            }
        }
    }

    private bool IsPathValid()
    {
        int cost = 0;
        for (int i = 1; i < _path.Count; i++)
        {
            if (((Vector2)_path[i]).DistanceSquaredTo((Vector2)_path[i - 1]) != 1)
            {
                return false;
            }
            switch (_map.GetCellv((Vector2)_path[i]))
            {
                case 0:
                    cost += 1;
                    break;
                case 1:
                    cost += 2;
                    break;
                default:
                    cost += 9001;
                    break;
            }
        }
        return cost <= _selectedUnit.MoveRange;
    }

    private void MoveSelectedUnit(Vector2 cellPosition)
    {
        if ((Vector2)_path[_path.Count - 1] != cellPosition)
        {
            _path.Add(cellPosition);
        }
        if (!IsPathValid())
        {
            _path = (Godot.Collections.Array)_pathfinding.Call("get_new_path", _selectedUnit.Cell.x, _selectedUnit.Cell.y, cellPosition.x, cellPosition.y, _map);
        }
        _playerUnits.Remove(_selectedUnit.Cell);
        _selectedUnit.Move(_path);
        _playerUnits.Add(_selectedUnit.Cell, _selectedUnit);
        _path.Clear();
        _pathOverlay.Clear();
        _pathArrow.Clear();
    }

    private void SelectUnit(Vector2 cellPosition)
    {
        _selectedUnit = _playerUnits[cellPosition];
        _path.Add(cellPosition);
        _pathArrow.DrawArrow(cellPosition);
        _movableCells = (Godot.Collections.Dictionary)_pathfinding.Call("get_movable_cells", cellPosition.x, cellPosition.y, _selectedUnit.MoveRange, _map);
        _pathOverlay.DrawOverlay(_movableCells);
    }

    private void CancelSelection()
    {
        _path.Clear();
        _pathOverlay.Clear();
        _pathArrow.Clear();
        _selectedUnit = null;
    }

    private InputType GetInputType(InputEvent @event)
    {
        if (_commandDialog.Visible)
        {
            return InputType.NOP;
        }
        if (@event is InputEventMouseButton)
        {
            InputEventMouseButton mouseButton = (InputEventMouseButton)@event;
            if (mouseButton.Pressed)
            {
                if (mouseButton.ButtonIndex == (int)ButtonList.Left)
                {
                    if (_enemyNeighbors.Count > 0)
                    {
                        return InputType.SELECT_ENEMY;
                    }
                    if (_selectedUnit != null)
                    {
                        return InputType.MOVE_UNIT;
                    }
                    return InputType.SELECT_UNIT;
                }
                if (mouseButton.ButtonIndex == (int)ButtonList.Right && _selectedUnit != null)
                {
                    return InputType.CANCEL;
                }
            }
        }
        if (@event is InputEventMouseMotion && _selectedUnit != null && _enemyNeighbors.Count == 0)
        {
            return InputType.DRAW_PATH;
        }
        return InputType.NOP;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouse mouseEvent)
        {
            Vector2 cellPosition = _map.WorldToMap(mouseEvent.Position);
            switch (GetInputType(@event))
            {
                case InputType.SELECT_UNIT:
                    if (_playerUnits.ContainsKey(cellPosition))
                    {
                        SelectUnit(cellPosition);
                    }
                    break;
                case InputType.SELECT_ENEMY:
                    if (_enemyNeighbors.ContainsKey(cellPosition))
                    {
                        foreach (Unit unit in _enemyNeighbors.Values)
                        {
                            unit.StopAnimation();
                        }
                        Combat(_selectedUnit, _enemyNeighbors[cellPosition]);
                        _enemyNeighbors.Clear();
                        _selectedUnit = null;
                    }
                    break;
                case InputType.MOVE_UNIT:
                    if (_movableCells.Contains(cellPosition) && !_playerUnits.ContainsKey(cellPosition))
                    {
                        MoveSelectedUnit(cellPosition);
                        GetEnemyNeighbors();
                        if (_enemyNeighbors.Count > 0)
                        {
                            _fireButton.Show();
                        }
                        else
                        {
                            _fireButton.Hide();
                        }
                        _commandDialog.RectPosition = _map.MapToWorldCenter(cellPosition);
                        _commandDialog.Show();
                    }
                    break;
                case InputType.CANCEL:
                    CancelSelection();
                    break;
                case InputType.DRAW_PATH:
                    if (_movableCells.Contains(cellPosition) && (Vector2)_path[_path.Count - 1] != cellPosition)
                    {
                        _path.Add(cellPosition);
                        if (IsPathValid())
                        {
                            _pathArrow.DrawArrow(cellPosition);
                        }
                        else
                        {
                            _path = (Godot.Collections.Array)_pathfinding.Call("get_new_path", _selectedUnit.Cell.x, _selectedUnit.Cell.y, cellPosition.x, cellPosition.y, _map);
                            _pathArrow.Clear();
                            foreach (Vector2 cell in _path)
                            {
                                _pathArrow.DrawArrow(cell);
                            }
                        }
                    }
                    break;
            }
        }
    }

    public void _EndTurnPressed()
    {
        foreach (Unit unit in GetNode("AIUnits").GetChildren())
        {
            Godot.Collections.Array aiPath = new Godot.Collections.Array();
            aiPath.Add(unit.Cell + new Vector2(-1, 0));
            aiPath.Add(unit.Cell + new Vector2(-2, 0));
            aiPath.Add(unit.Cell + new Vector2(-2, -1));
            unit.Move(aiPath);
        }
    }

    public void _CommandDialogPressed(int iCommand)
    {
        Command command = (Command)iCommand;
        if (command == Command.FIRE)
        {
            foreach (Unit unit in _enemyNeighbors.Values)
            {
                unit.StrobeAnimation();
            }
        }
        else
        {
            _selectedUnit = null;
            _enemyNeighbors.Clear();
        }
        _commandDialog.Hide();
    }

    public void UnitRemoved(Unit unit)
    {
        foreach (KeyValuePair<Vector2, Unit> kvp in _playerUnits)
        {
            if (kvp.Value == unit)
            {
                _playerUnits.Remove(kvp.Key);
                return;
            }
        }
        foreach (KeyValuePair<Vector2, Unit> kvp in _enemyUnits)
        {
            if (kvp.Value == unit)
            {
                _enemyUnits.Remove(kvp.Key);
                return;
            }
        }
    }
}
