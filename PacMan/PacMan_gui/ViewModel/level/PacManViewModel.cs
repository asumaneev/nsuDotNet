﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using PacMan_gui.Annotations;
using PacMan_model.level.cells.pacman;
using PacMan_model.util;
using Point = PacMan_model.util.Point;

namespace PacMan_gui.ViewModel.level {
    internal class PacManViewModel : INotifyPropertyChanged {

        public Point Position { get; private set; }
        public Direction Direction { get; private set; }
        
        public int LivesNumber {
            get { return _livesNumber; }
            private set {
                if (value == _livesNumber) {
                    return;
                }
                _livesNumber = value;
                OnPropertyChanged();
            }
        }


        public FieldViewModel FieldViewModel { get; set; }

        private readonly Canvas _canvas;
        private IPacManObserverable _pacMan;
        private Shape _addedShape;
        private int _livesNumber;

        public PacManViewModel(IPacManObserverable pacMan, Canvas canvas, FieldViewModel fieldViewModel) {
            if (null == pacMan) {
                throw new ArgumentNullException("pacMan");
            }
            if (null == canvas) {
                throw new ArgumentNullException("canvas");
            }
            if (null == fieldViewModel) {
                throw new ArgumentNullException("fieldViewModel");
            }

            _canvas = canvas;
            Init(pacMan, fieldViewModel);
            
            //Redraw();
        }

        public void Init(IPacManObserverable pacMan, FieldViewModel fieldViewModel) {
            if (null == pacMan) {
                throw new ArgumentNullException("pacMan");
            }
            if (null == fieldViewModel) {
                throw new ArgumentNullException("fieldViewModel");
            }

            _pacMan = pacMan;
            pacMan.PacmanState += OnPacManChanged;

            FieldViewModel = fieldViewModel;
        }

        private void OnPacManChanged(Object sender, PacmanStateChangedEventArgs e) {

            if (null == e) {
                throw new ArgumentNullException("e");
            }

            Position = e.Position;
            Direction = e.Direction;
            LivesNumber = e.Lives;

            _canvas.Dispatcher.BeginInvoke(DispatcherPriority.Send, new WorkWithCanvas(RedrawPacManOnCanvas));
        }

        public void Redraw() {
            _pacMan.ForceNotify();
        }

        private delegate void WorkWithCanvas();

        private void RedrawPacManOnCanvas() {
            ClearCanvas();
            

            double cellWidth = _canvas.ActualWidth / FieldViewModel.Width;
            double cellHeight = _canvas.ActualHeight / FieldViewModel.Height;
            _addedShape = CellToView.PacmanToShape(cellWidth, cellHeight, Position.GetX(), Position.GetY());

            _canvas.Children.Add(_addedShape);
        }

        private void ClearCanvas() {
            if ((null != _addedShape) && _canvas.Children.Contains(_addedShape)) {

                _canvas.Children.Remove(_addedShape);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
