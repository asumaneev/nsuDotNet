﻿using System;
using System.Collections.Generic;
using System.Linq;
using PacMan_model.level.cells;
using PacMan_model.util;

namespace PacMan_model.level.field {
    internal sealed class Field : IField {
        private int _width;
        private int _height;

        //  number of cells with Dots (PacDot, Energizer ...)
        private int _numberOfDots;

        private readonly Wall _wallAroundField = new Wall(new Point(0, 0));

        //  x - width
        //  y - height
        //  cell at (x, y)-position is cells.at(y * width + x)
        private IList<StaticCell> _cells;

        #region Initialization

        public Field() {}

        public Field(int width, int height, IList<StaticCell> cells) {
            Init(width, height, cells);
        }

        public void Init(int width, int height, IList<StaticCell> cells) {
            if (null == cells) {
                throw new ArgumentNullException("cells");
            }

            if (width <= 0) {
                throw new ArgumentOutOfRangeException("width");
            }
            if (height <= 0) {
                throw new ArgumentOutOfRangeException("height");
            }
            if (width * height != cells.Count) {
                throw new ArgumentException("Field initialization: invalid size of cells list");
            }


            _width = width;
            _height = height;

            _cells = cells;

            CalculateDots();

            //NotifyChangedStatement();
        }

        #endregion

        #region Disposing

        public void Dispose() {
            UnsubsrcibeAll();

//            _cells.Clear();
//
//            _width = 0;
//            _height = 0;
        }

        private void UnsubsrcibeAll() {
            if (null != FieldState) {
                foreach (var levelClient in FieldState.GetInvocationList()) {
                    FieldState -= levelClient as EventHandler<FieldStateChangedEventArs>;
                }
            }

            if (null != DotsEnds) {
                foreach (var levelClient in DotsEnds.GetInvocationList()) {
                    DotsEnds -= levelClient as EventHandler;
                }
            }
        }

        #endregion

        #region Getters

        public int GetWidth() {
            return _width;
        }

        public int GetHeight() {
            return _height;
        }

        public int GetNumberOfDots() {
            return _numberOfDots;
        }

        public StaticCell GetCell(int x, int y) {
            return !ContainsCellAtPoint(x, y) ? _wallAroundField : _cells[y * _width + x];
        }

        public StaticCell GetCell(Point p) {
            if (null == p) {
                throw new ArgumentNullException("p");
            }

            return GetCell(p.GetX(), p.GetY());
        }

        public IList<StaticCell> GetCells() {
            return _cells;
        }

        public IEnumerable<StaticCell> GetNeighbors(Point cellPoint) {
            if (null == cellPoint) {
                throw new ArgumentNullException("cellPoint");
            }

            return GetNeighborsPoints(cellPoint).Select(GetCell);
        }

        public IEnumerable<Point> GetNeighborsPoints(Point cellPoint) {
            if (null == cellPoint) {
                throw new ArgumentNullException("cellPoint");
            }

            return Enumerable.Repeat(cellPoint, Direction.Directions.Length)
                .Zip(Direction.Directions, (point, direction) => direction.GetNear(point));
        }

//        public IEnumerable<StaticCell> GetRemoteNeighbors(Point cellPoint, int distance) {
//            if (null == cellPoint) {
//                throw new ArgumentNullException("cellPoint");
//            }
//            if ((0 >= distance) || (distance >= (_width - cellPoint.GetX()))
//                || (distance >= (_height - cellPoint.GetY()))) {
//                throw new ArgumentOutOfRangeException("distance");
//            }
//            return GetRemoteNeighborsPoints(cellPoint, distance).Select(GetCell);
//        }
//
//        public IEnumerable<Point> GetRemoteNeighborsPoints(Point cellPoint, int distance) {
//            if (null == cellPoint) {
//                throw new ArgumentNullException("cellPoint");
//            }
//            if ((0 >= distance) || (distance >= (_width - cellPoint.GetX()))
//                || (distance >= (_height - cellPoint.GetY()))) {
//                throw new ArgumentOutOfRangeException("distance");
//            }
//            if (!ContainsCellAtPoint(cellPoint)) {
//                throw new ArgumentException("field does not contain sent cell");
//            }
//
//
//            return Direction.GetSquareAround(cellPoint, distance);
//        }

        private bool ContainsCellAtPoint(Point cellPoint) {
            if (null == cellPoint) {
                throw new ArgumentNullException("cellPoint");
            }

            return ContainsCellAtPoint(cellPoint.GetX(), cellPoint.GetY());
        }

        private bool ContainsCellAtPoint(int x, int y) {
            return ((x >= 0) && (y >= 0) && (x < _width) && (y < _height));
        }

        #endregion

        #region Setters

        public void SetCell(int x, int y, StaticCell cell) {
            if (null == cell) {
                throw new ArgumentNullException("cell");
            }

            if ((x >= _width) || (y >= _height)) {
                throw new CellOutOfField(new Point(x, y));
            }


            //  if old cell is cell with cost
            //  if new cell is not cell with cost
            //  decrement number of cells with costs
            if ((_cells[y * _width + x] is ICellWithCost) && (false == (cell is ICellWithCost))) {
                --_numberOfDots;
            }
                //  if old cell is not cell with cost
                //  if new cell is cell with cost
                //  increment number of cell 
            else if ((false == (_cells[y * _width + x] is ICellWithCost)) && (cell is ICellWithCost)) {
                ++_numberOfDots;
            }

            _cells[y * _width + x] = cell;

            NotifyChangedStatement();
        }

        public void SetSell(Point p, StaticCell cell) {
            if (null == p) {
                throw new ArgumentNullException("p");
            }
            if (null == cell) {
                throw new ArgumentNullException("cell");
            }
            SetCell(p.GetX(), p.GetY(), cell);
        }

        private void CalculateDots() {
            // ReSharper disable once UnusedVariable
            foreach (var cell in _cells.OfType<ICellWithCost>()) {
                ++_numberOfDots;
            }
        }

        #endregion

        #region Events

        public event EventHandler<FieldStateChangedEventArs> FieldState;
        public event EventHandler DotsEnds;

        public void ForceNotify() {
            NotifyChangedStatement();
        }

        private void OnStatementChangedNotify(FieldStateChangedEventArs e) {
            if (null == e) {
                throw new ArgumentNullException("e");
            }

            e.Raise(this, ref FieldState);
        }

        private void OnDotsEnds() {
            EventArgs.Empty.Raise(this, ref DotsEnds);
        }

        private void NotifyChangedStatement() {
            if (0 == _numberOfDots) {
                OnDotsEnds();
            }
            else {
                OnStatementChangedNotify(new FieldStateChangedEventArs(this));
            }
        }

        #endregion
    }
}