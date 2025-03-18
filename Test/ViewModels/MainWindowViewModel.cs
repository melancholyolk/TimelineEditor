using Livet;
using Livet.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Test.Utilities;
using Test.Views;

namespace Test.ViewModels
{
    public class TimelineKeyViewModel : ViewModel
    {
        //缩放为1时的位置，帧数
        public uint ActualPosition { get; set; }

        //缩放为1时的长度，帧数
        public uint ActualLength { get; set; }
        public double PlacementPosition
        {
            get => _PlacementPosition;
            set => RaisePropertyChangedIfSet(ref _PlacementPosition, value);
        }
        double _PlacementPosition = 0;

        public bool IsSelected
        {
            get => _IsSelected;
            set => RaisePropertyChangedIfSet(ref _IsSelected, value);
        }
        bool _IsSelected = true;

        public double KeyLength
        {
            get => _KeyLength;
            set => RaisePropertyChangedIfSet(ref _KeyLength, value);
        }
        double _KeyLength = 360;
        public TimelineKeyViewModel(uint actualPosition, uint actualLength, double placementPosition, double keyLength)
        {
            ActualPosition = actualPosition;
            ActualLength = actualLength;
            PlacementPosition = placementPosition;
            KeyLength = keyLength;
        }
    }

    public class TrackItemViewModel : ViewModel
    {
        public string Name
        {
            get => _Name;
            set => RaisePropertyChangedIfSet(ref _Name, value);
        }
        string _Name = string.Empty;

        public bool IsSelected
        {
            get => _IsSelected;
            set => RaisePropertyChangedIfSet(ref _IsSelected, value);
        }
        bool _IsSelected;

        public IEnumerable<TimelineKeyViewModel> Keys => _Keys;
        ObservableCollection<TimelineKeyViewModel> _Keys = new ObservableCollection<TimelineKeyViewModel>();

        public ViewModelCommand AddKeyCommand => _AddKeyCommand.Get(AddKey, () => Owner.IsPlaying == false);
        ViewModelCommandHandler _AddKeyCommand = new ViewModelCommandHandler();

        public ViewModelCommand AddKeyWithDoubleClickCommand => _AddKeyWithDoubleClickCommand.Get(AddKeyWithDoubleClick, () => Owner.IsPlaying == false);
        ViewModelCommandHandler _AddKeyWithDoubleClickCommand = new ViewModelCommandHandler();


        MainWindowViewModel Owner { get; }

        public TrackItemViewModel(string name, MainWindowViewModel owner)
        {
            Name = name;
            Owner = owner;

            CompositeDisposable.Add(() =>
            {
                foreach (var key in _Keys)
                {
                    key.Dispose();
                }
            });
        }

        public void RaiseCanExecuteCommand()
        {
            AddKeyCommand.RaiseCanExecuteChanged();
            AddKeyWithDoubleClickCommand.RaiseCanExecuteChanged();
        }
        public void AddKey() => AddKey(1);
        public void AddKey(uint length = 1)
        {
            var frameCount = Owner.Viewer.TimelineEditor.GetPositionFrame(Owner.ContextMenuOpeningPosition.X);
            var key = new TimelineKeyViewModel(frameCount, length, Owner.Viewer.TimelineEditor.GetRulerPosition(frameCount), length * Owner.Viewer.TimelineEditor.SubHeaderDistance);
            AddKey(key);
        }

        public void AddKeyWithDoubleClick()
        {
            var frameCount = Owner.Viewer.TimelineEditor.GetPositionFrame(Owner.MousePositionOnTimelineLane.X);
            var key = new TimelineKeyViewModel(frameCount, 1, Owner.Viewer.TimelineEditor.GetRulerPosition(frameCount), 1 * Owner.Viewer.TimelineEditor.SubHeaderDistance);
            AddKey(key);
        }

        public void AddKey(TimelineKeyViewModel vm)
        {
            foreach (var key in _Keys)
            {
                key.IsSelected = false;
            }
            _Keys.Add(vm);
        }

        public void DeleteSelectedKeys()
        {
            var removeKeys = _Keys.Where(arg => arg.IsSelected).ToArray();

            foreach (var removeKey in removeKeys)
            {
                removeKey.Dispose();
                _Keys.Remove(removeKey);
            }
        }
    }

    public class MainWindowViewModel : ViewModel
    {
        public MainWindow Viewer { get; private set; }
        public IEnumerable<TrackItemViewModel> Tracks => _Tracks;
        ObservableCollection<TrackItemViewModel> _Tracks = new ObservableCollection<TrackItemViewModel>();

        public ViewModelCommand AddTrackCommand => _AddTrackCommand.Get(AddTrack);
        ViewModelCommandHandler _AddTrackCommand = new ViewModelCommandHandler();

        public ViewModelCommand RemoveTracksCommand => _RemoveTracksCommand.Get(RemoveTracks);
        ViewModelCommandHandler _RemoveTracksCommand = new ViewModelCommandHandler();

        public ViewModelCommand AddKeyCommand => _AddKeyCommand.Get(AddKey);
        ViewModelCommandHandler _AddKeyCommand = new ViewModelCommandHandler();

        public ListenerCommand<Point> BeginKeyMovingCommand => _BeginKeyMovingCommand.Get(BeginKeyMoving);
        ViewModelCommandHandler<Point> _BeginKeyMovingCommand = new ViewModelCommandHandler<Point>();

        public ListenerCommand<Point> KeyMovingCommand => _KeyMovingCommand.Get(KeyMoving);
        ViewModelCommandHandler<Point> _KeyMovingCommand = new ViewModelCommandHandler<Point>();

        public ListenerCommand<Point> EndKeyMovingCommand => _EndKeyMovingCommand.Get(EndKeyMoving);
        ViewModelCommandHandler<Point> _EndKeyMovingCommand = new ViewModelCommandHandler<Point>();
        public ViewModelCommand LaneClickedCommand => _LaneClickedCommand.Get(LaneClicked);
        ViewModelCommandHandler _LaneClickedCommand = new ViewModelCommandHandler();

        public ViewModelCommand DeleteOnTrackCommand => _DeleteOnTrackCommand.Get(DeleteOnTrack);
        ViewModelCommandHandler _DeleteOnTrackCommand = new ViewModelCommandHandler();

        public ViewModelCommand DeleteOnLaneCommand => _DeleteOnLaneCommand.Get(DeleteOnLane);
        ViewModelCommandHandler _DeleteOnLaneCommand = new ViewModelCommandHandler();

        public ViewModelCommand SwitchPlayingStateCommand => _SwitchPlayingStateCommand.Get(SwitchPlayingState);
        ViewModelCommandHandler _SwitchPlayingStateCommand = new ViewModelCommandHandler();

        public ViewModelCommand ResetTimeCommand => _ResetTimeCommand.Get(ResetTime);
        ViewModelCommandHandler _ResetTimeCommand = new ViewModelCommandHandler();

        public ListenerCommand<double> ScaleChangedCommand => _ScaleChangedCommand.Get(ScaleChanged);
        ViewModelCommandHandler<double> _ScaleChangedCommand = new ViewModelCommandHandler<double>();
        public bool IsPlaying
        {
            get => _IsPlaying;
            set => RaisePropertyChangedIfSet(ref _IsPlaying, value);
        }
        bool _IsPlaying;

        public bool IsDisplayMarkerAlways
        {
            get => _IsDisplayMarkerAlways;
            set => RaisePropertyChangedIfSet(ref _IsDisplayMarkerAlways, value);
        }
        bool _IsDisplayMarkerAlways;

        public double CurrentTime
        {
            get => _CurrentTime;
            set => RaisePropertyChangedIfSet(ref _CurrentTime, value);
        }
        double _CurrentTime = 0;

        public Point MousePositionOnTimelineLane
        {
            get => _MousePositionOnTimelineLane;
            set => RaisePropertyChangedIfSet(ref _MousePositionOnTimelineLane, value);
        }
        Point _MousePositionOnTimelineLane;

        public Point ContextMenuOpeningPosition
        {
            get => _ContextMenuOpeningPosition;
            set => RaisePropertyChangedIfSet(ref _ContextMenuOpeningPosition, value);
        }
        Point _ContextMenuOpeningPosition;

        Point _CapturedBaseKeyPosition = new Point(0, 0);

        TimelineKeyViewModel[]? _SelectedKeyVMs = null;
        double[]? _SelectedKeyOffsetPlacements = null;
        double scale = 1;
        public MainWindowViewModel(MainWindow mainWindow)
        {
            Viewer = mainWindow;
            // ViewModelCommand.RaiseCanExecuteChangedの実行に必要
            DispatcherHelper.UIDispatcher = Application.Current.Dispatcher;

            var test1Lane = new TrackItemViewModel("test1", this);
            test1Lane.AddKey(10);
            test1Lane.Keys.ElementAt(0).IsSelected = true;

            _Tracks.Add(test1Lane);
            _Tracks.Add(new TrackItemViewModel("test2", this));
        }

        void AddTrack()
        {
            _Tracks.Add(new TrackItemViewModel($"test{_Tracks.Count}", this));
        }

        void RemoveTracks()
        {
            var removeTracks = _Tracks.Where(arg => arg.IsSelected).ToArray();

            foreach (var removeTrack in removeTracks)
            {
                _Tracks.Remove(removeTrack);
            }
        }

        public void AddKey()
        {
            var addKeyTracks = _Tracks.Where(arg => arg.IsSelected);

            foreach (var addKeyTrack in addKeyTracks)
            {
                var frameCount = Viewer.TimelineEditor.GetPositionFrame(ContextMenuOpeningPosition.X);
                var key = new TimelineKeyViewModel(frameCount, 1, Viewer.TimelineEditor.GetRulerPosition(frameCount), 1 * Viewer.TimelineEditor.SubHeaderDistance);
                addKeyTrack.AddKey(key);
            }
        }

        void BeginKeyMoving(Point pos)
        {
            _CapturedBaseKeyPosition = pos;
            _SelectedKeyVMs = _Tracks.SelectMany(arg => arg.Keys.Where(key => key.IsSelected)).ToArray();
            _SelectedKeyOffsetPlacements = _SelectedKeyVMs.Select(arg => arg.PlacementPosition).ToArray();
        }

        void KeyMoving(Point pos)
        {
            if (_SelectedKeyVMs == null || _SelectedKeyOffsetPlacements == null)
            {
                throw new InvalidProgramException();
            }

            var delta = pos.X - _CapturedBaseKeyPosition.X;
            for (int i = 0; i < _SelectedKeyVMs.Length; i++)
            {
                var key = _SelectedKeyVMs[i];
                var offset = _SelectedKeyOffsetPlacements[i];
                key.PlacementPosition = Viewer.TimelineEditor.GetRulerPosition(offset + delta);
                key.ActualPosition = Viewer.TimelineEditor.GetPositionFrame(key.PlacementPosition);
            }
        }

        void EndKeyMoving(Point pos)
        {
            _SelectedKeyVMs = null;
            _SelectedKeyOffsetPlacements = null;
        }

        void LaneClicked()
        {
            foreach (var key in _Tracks.SelectMany(arg => arg.Keys))
            {
                key.IsSelected = false;
            }
        }

        void DeleteOnTrack()
        {
            var removeTracks = _Tracks.Where(arg => arg.IsSelected).ToArray();
            foreach (var removeTrack in removeTracks)
            {
                removeTrack.Dispose();
                _Tracks.Remove(removeTrack);
            }
        }

        void DeleteOnLane()
        {
            foreach (var track in _Tracks)
            {
                track.DeleteSelectedKeys();
            }
        }

        void SwitchPlayingState()
        {
            IsPlaying = !IsPlaying;

            foreach (var track in _Tracks)
            {
                track.RaiseCanExecuteCommand();
            }
        }

        void ResetTime()
        {
            CurrentTime = 0;
        }
        void ScaleChanged(double newScale)
        {
            scale = newScale;
            var keys = _Tracks.SelectMany(x => x.Keys);
            foreach (var key in keys)
            {
                key.PlacementPosition = Viewer.TimelineEditor.GetRulerPosition(key.ActualPosition);
                key.KeyLength = key.ActualLength * Viewer.TimelineEditor.SubHeaderDistance;
            }
        }
    }
}
