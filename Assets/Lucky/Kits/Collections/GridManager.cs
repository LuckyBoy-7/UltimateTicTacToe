using System;
using System.Collections.Generic;
using AceAttorney.Scripts.Card.Logic;
using Lucky.Kits.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Lucky.Kits.Collections
{
    /// <summary>
    /// 一个格子只能存一个item, 并且一个item也只能在一个位置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class GridManager<T>
    {
        [ShowInInspector] private DefaultDict<Vector2Int, T> gridPosToItem = new DefaultDict<Vector2Int, T>(() => default);
        [ShowInInspector] private DefaultDict<T, Vector2Int> itemToGridPos = new DefaultDict<T, Vector2Int>(() => default);
        private float cellWidth;
        private float cellHeight;
        private bool doOffset;

        private Vector2 CellSize => new Vector2(cellWidth, cellHeight);
        private Vector2 HalfCellSize => CellSize / 2;
        private Vector3 Origin => (doOffset ? -HalfCellSize : Vector2.zero) + (pivotPosFunc?.Invoke() ?? Vector2.zero);

        public Vector2Int ToGridPos(Vector3 position) => ((position - Origin) / CellSize).FloorToVector2Int();
        public Vector3 ToWorldPos(Vector2Int gridPos) => Origin + (Vector3)HalfCellSize + (Vector3)(gridPos * CellSize);
        public Vector3 ToCenterPos(Vector3 position) => ToWorldPos(ToGridPos(position));
        public Vector2Int ToGridPos(T item) => itemToGridPos[item];
        private Func<Vector2> pivotPosFunc;

        public GridManager(float cellWidth, float cellHeight, bool doOffset = true, Func<Vector2> pivotPosFunc = null)
        {
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            this.doOffset = doOffset;
            this.pivotPosFunc = pivotPosFunc;
        }

        public void SetItem(T item, Vector2Int gridPos)
        {
            gridPosToItem[gridPos] = item;
            itemToGridPos[item] = gridPos;
        }

        public void DiscardItem(T item, Vector2Int gridPos)
        {
            gridPosToItem[gridPos] = default;
            itemToGridPos[item] = Vector2Int.zero;
        }

        public T this[int x, int y]
        {
            get => gridPosToItem[new Vector2Int(x, y)];
            set => SetItem(value, new Vector2Int(x, y));
        }

        public T this[Vector2Int gridPosition]
        {
            get => gridPosToItem[gridPosition];
            set => SetItem(value, gridPosition);
        }

        public void Swap(Vector2Int pos0, Vector2Int pos1) => (this[pos0], this[pos1]) = (this[pos1], this[pos0]);

        #region Vector2

        public void SetItem(T item, Vector2 position)
        {
            gridPosToItem[ToGridPos(position)] = item;
            itemToGridPos[item] = ToGridPos(position);
        }

        public void DiscardItem(T item, Vector2 position)
        {
            gridPosToItem[ToGridPos(position)] = default;
            itemToGridPos[item] = Vector2Int.zero;
        }

        public T this[float x, float y]
        {
            get => gridPosToItem[ToGridPos(new Vector2(x, y))];
            set => SetItem(value, ToGridPos(new Vector2(x, y)));
        }

        public T this[Vector2 position]
        {
            get => gridPosToItem[ToGridPos(position)];
            set => SetItem(value, ToGridPos(position));
        }

        public void Swap(Vector2 pos0, Vector2 pos1) => (this[pos0], this[pos1]) = (this[pos1], this[pos0]);

        #endregion
    }
}