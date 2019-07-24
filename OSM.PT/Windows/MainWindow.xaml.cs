using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Demo.WindowsForms;
using Demo.WindowsPresentation.Cash;
using Demo.WindowsPresentation.Controls;
using Demo.WindowsPresentation.CustomMarkers;
using Demo.WindowsPresentation.Source;
using Demo.WindowsPresentation.Tracking.Registrator;
using Demo.WindowsPresentation.Tracking.Telemetry;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Microsoft.Windows.Controls.Ribbon;
using OsmSharp.Osm;
using OsmSharp.Osm.Data;
using OsmSharp.Osm.Data.Core.Processor.Filter.Sort;
using OsmSharp.Osm.Data.PBF.Raw.Processor;
using OsmSharp.Osm.Data.SQLServer.SimpleSchema;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.DynamicGraph.PreProcessed;
using OsmSharp.Routing.Graph.DynamicGraph.SimpleWeighed;
using OsmSharp.Routing.Graph.Memory;
using OsmSharp.Routing.Graph.Router.Dykstra;
using OsmSharp.Routing.Osm.Data;
using OsmSharp.Routing.Osm.Data.Processing;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Route;
using OsmSharp.Tools.Math.Geo;
using Rodsoft.RegistryOperations;
using RodSoft.OSM.PT.Online.Controls;
using RodSoft.OSM.Source;
using RodSoft.OSM.Tracking;
using RodSoft.OSM.Tracking.Controls;
using RodSoft.OSMPT.PT.Online;

namespace Demo.WindowsPresentation
{
    public partial class MainWindow : Window
    {
        PointLatLng start;
        PointLatLng end;

        // marker
        GMapMarker currentMarker;

        // zones list
        List<GMapMarker> Circles = new List<GMapMarker>();
        public MainWindow()
        {
            InitializeComponent();

            // add your custom map db provider
            //MySQLPureImageCache ch = new MySQLPureImageCache();
            //ch.ConnectionString = @"server=sql2008;User Id=trolis;Persist Security Info=True;database=gmapnetcache;password=trolis;";
            //MainMap.Manager.SecondaryCache = ch;

            // set your proxy here if need
            //GMapProvider.WebProxy = new WebProxy("10.2.0.100", 8080);
            //GMapProvider.WebProxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeada");
            string q = "абвг";
            byte[] winByte = System.Text.Encoding.UTF8.GetBytes(q);
            string s = System.Text.Encoding.UTF8.GetString(winByte);//GetEncoding(1251)

            // set cache mode only if no internet avaible
            if (!Stuff.PingNetwork("openstreetmap.org"))
            {
                MainMap.Manager.Mode = AccessMode.CacheOnly;
                MessageBox.Show("No internet connection available, going to CacheOnly mode.", "GMap.NET - Demo.WindowsPresentation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // config map
            MainMap.MapProvider = GMapProviders.OpenStreetMap;
            MainMap.Position = new PointLatLng(56.844, 60.634);
            MainMap.Zoom = 12;
            MainMap.ShowTileGridLines = false;

            //MainMap.ScaleMode = ScaleModes.Dynamic;

            // map events
            MainMap.OnPositionChanged += new PositionChanged(MainMap_OnCurrentPositionChanged);
            MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
            MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
            MainMap.OnMapTypeChanged += new MapTypeChanged(MainMap_OnMapTypeChanged);
            MainMap.MouseMove += new System.Windows.Input.MouseEventHandler(MainMap_MouseMove);
            MainMap.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(MainMap_MouseLeftButtonDown);
            MainMap.MouseEnter += new MouseEventHandler(MainMap_MouseEnter);
            MainMap.OnMapZoomChanged += new MapZoomChanged(ZoomChanged);

            // get map types
            comboBoxMapType.ItemsSource = GMapProviders.List;
            comboBoxMapType.DisplayMemberPath = "Name";
            comboBoxMapType.SelectedItem = MainMap.MapProvider;

            //MapProviderMenu.ItemsSource = GMapProviders.List;
            //MapProviderMenu.DisplayMemberPath = "Name";

            foreach (GMapProvider provider in GMapProviders.List)
            {
                RibbonMenuItem menuItem = new RibbonMenuItem();
                menuItem.Click += menuItem_Click;
                menuItem.Header = provider.Name;
                menuItem.Tag = provider;
                MapProviderMenu.Items.Add(menuItem);
            }

            // acccess mode
            comboBoxMode.ItemsSource = Enum.GetValues(typeof(AccessMode));
            comboBoxMode.SelectedItem = MainMap.Manager.Mode;

            // get cache modes
            checkBoxCacheRoute.IsChecked = MainMap.Manager.UseRouteCache;
            checkBoxGeoCache.IsChecked = MainMap.Manager.UseGeocoderCache;

            // setup zoom min/max
            sliderZoom.Maximum = MainMap.MaxZoom;
            sliderZoom.Minimum = MainMap.MinZoom;

            // get position
            textBoxLat.Text = MainMap.Position.Lat.ToString(CultureInfo.InvariantCulture);
            textBoxLng.Text = MainMap.Position.Lng.ToString(CultureInfo.InvariantCulture);
            // get marker state
            checkBoxCurrentMarker.IsChecked = false;


            // can drag map
            checkBoxDragMap.IsChecked = MainMap.CanDragMap;

#if DEBUG
            checkBoxDebug.IsChecked = false;
#endif

            //validator.Window = this;

            // set current marker
            currentMarker = new GMapMarker(MainMap.Position);
            {
                currentMarker.Shape = new CustomMarkerRed(this, currentMarker, "custom position marker");
                currentMarker.Offset = new System.Windows.Point(-15, -15);
                currentMarker.ZIndex = int.MaxValue;
                MainMap.Markers.Add(currentMarker);
            }

            //if(false)
            {
                // add my city location for demo
                GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;

                PointLatLng? city = GMapProviders.GoogleMap.GetPoint("Россия, Екатеринбург", out status);
                if (city != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                {
                    GMapMarker it = new GMapMarker(city.Value);
                    {
                        it.ZIndex = 55;
                        it.Shape = new CustomMarkerDemo(this, it, "Welcome to Lithuania! ;}");
                    }
                    //               MainMap.Markers.Add(it);
                    /*
                                   #region -- add some markers and zone around them --
                                   //if(false)
                                   {
                                      List<PointAndInfo> objects = new List<PointAndInfo>();
                                      {
                                         string area = "Antakalnis";
                                         PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                                         if(pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                                         {
                                            objects.Add(new PointAndInfo(pos.Value, area));
                                         }
                                      }
                                      {
                                         string area = "Senamiestis";
                                         PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                                         if(pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                                         {
                                            objects.Add(new PointAndInfo(pos.Value, area));
                                         }
                                      }
                                      {
                                         string area = "Pilaite";
                                         PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                                         if(pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                                         {
                                            objects.Add(new PointAndInfo(pos.Value, area));
                                         }
                                      }
                                      AddDemoZone(8.8, city.Value, objects);
                                   }
                                   #endregion
                    */
                }

                if (MainMap.Markers.Count > 1)
                {
                    MainMap.ZoomAndCenterMarkers(null);
                }
            }

            /*
             *       foreach (OSMStopPoint point in _Route.Stops)
                     {
                         currentMarker = new GMapMarker(new PointLatLng(point.Latitude, point.Longtitude));
                         currentMarker.Shape = new StopVisual(currentMarker, new SolidColorBrush(Color.FromRgb(200, 0, 0)));
                         MainMap.Markers.Add(currentMarker);
                     }
                     /*          List<Point> points = new List<Point>();
                               foreach(OSMWay way in route.Ways)
                               {
                                   foreach(OSMPoint point in way.Points)
                                   {
                                     points.Add(new Point(point.Latitude, point.Longtitude));
                                   }
                               }
                               System.Windows.Shapes.Path path = MainMap.CreateRoutePath(points);
                             */
            // perfromance test
            timer.Interval = TimeSpan.FromMilliseconds(44);
            timer.Tick += new EventHandler(timer_Tick);

            timerShowCurrentPosition.Interval = TimeSpan.FromMilliseconds(1000);
            timerShowCurrentPosition.Tick += new EventHandler(RegisterGeoPosition);
            timerShowCurrentPosition.Start();

            // transport demo
            transport.DoWork += new DoWorkEventHandler(transport_DoWork);
            transport.ProgressChanged += new ProgressChangedEventHandler(transport_ProgressChanged);
            transport.WorkerSupportsCancellation = true;
            transport.WorkerReportsProgress = true;

            _loadBackground.DoWork += new DoWorkEventHandler(Do_LoadRoutes);
            _loadBackground.ProgressChanged += new ProgressChangedEventHandler(loadStops_ProgressChanged);
            _loadBackground.WorkerSupportsCancellation = true;
            _loadBackground.WorkerReportsProgress = true;
            //   ZoomChanged();
        }

        void menuItem_Click(object sender, RoutedEventArgs e)
        {
            MainMap.MapProvider = (GMapProvider)((RibbonMenuItem)sender).Tag;
        }

        private void Do_LoadRoutes(object sender, DoWorkEventArgs e)
        {
            LoadRoutes();
        }

        IList<RectLatLng> _DrawedRects = new List<RectLatLng>();

        private bool _IsFirstStopLoad = true;

        private bool IsViewAreaDrawed(RectLatLng viewArea)
        {
            foreach (RectLatLng rect in _DrawedRects)
            {
                if (rect.Contains(viewArea))
                    return true;
            }
            return false;
        }

        private void LoadRoutes()
        {
            CashHelper cash = CashHelper.Cash;

            // Cash.CashHelper.CreateDB();
            if (LoadRoutesService.LoadRoutes(cash, MainMap.ViewArea) || !IsViewAreaDrawed(MainMap.ViewArea))
            {
                /*
                //         _Route = ReadOSMRoute.ReadRoute();
                _Route = cash.ReadRoute(376588);
                IList<OSMStopPoint> stopPoints = cash.ReadStops();
                lock (_NewStopPoints)
                {
                    foreach (OSMStopPoint point in stopPoints)
                    {
                        if (_NewStopPoints.ContainsKey(point.ID))
                            continue;
                        _NewStopPoints.Add(point.ID, point);
                    }
                }
                 * 
                 * 
                 * */
                _DrawingRect = new RectLatLng(Math.Ceiling(MainMap.ViewArea.Lat), Math.Truncate(MainMap.ViewArea.Lng), Math.Ceiling(MainMap.ViewArea.Lng + MainMap.ViewArea.WidthLng) - Math.Truncate(MainMap.ViewArea.Lng), Math.Ceiling(MainMap.ViewArea.Lat) - Math.Truncate(MainMap.ViewArea.Lat - MainMap.ViewArea.HeightLat));
                _DrawingStopPoints = CashHelper.Cash.ReadStops(_DrawingRect);
                AddStopPoints(_DrawingStopPoints);
                _loadBackground.ReportProgress(100);
                _IsFirstStopLoad = false;
            }
        }
        IList<OSMStopPoint> _DrawingStopPoints;
        RectLatLng _DrawingRect;
        private IDictionary<long, OSMStopPoint> _NewStopPoints = new Dictionary<long, OSMStopPoint>();
        private IDictionary<OSMStopPoint, GMapMarker> _StopPoints = new Dictionary<OSMStopPoint, GMapMarker>();

        private void DrawStops(IEnumerable<OSMStopPoint> stopPoints)
        {
            lock (_StopPoints)
            {
                foreach (OSMStopPoint point in stopPoints)
                {
                    if (_StopPoints.Keys.Where(stopPoint => stopPoint.ID == point.ID).Count() > 0)
                        continue;
                    currentMarker = new GMapMarker(new PointLatLng(point.Latitude, point.Longtitude));
                    Color c = Color.FromRgb(200, 0, 0);
                    if (Tools.TestBit(point.StopType, (int)StopTypes.StopPoint))
                        c = Color.FromRgb(0, 250, 0);
                    if (Tools.TestBit(point.StopType, (int)StopTypes.TramStop))
                        c = Color.FromRgb(255, 201, 14);
                    if (Tools.TestBit(point.StopType, (int)StopTypes.RailroadStation))
                        c = Color.FromRgb(0, 0, 0);
                    if (Tools.TestBit(point.StopType, (int)StopTypes.Platform))
                        c = Color.FromRgb(150, 150, 0);
                    StopVisual v = new StopVisual(currentMarker, new SolidColorBrush(c));
                    if (Tools.TestBit(point.StopType, (int)StopTypes.StopPoint))
                    {
                        v.Height = v.Width = 2;
                    }
                    v.IsTextVisible = false;

                    v.Text = point.Name;
                    v.Tag = point;
                    v.ShowStopInfo += new ShowStopInfoDelegate(StopMarker_ShowStopInfo);
                    v.HideStopInfo += new ShowStopInfoDelegate(StopMarker_HideStopInfo);
                    //                   v.Tooltip.SetValues(point);
                    v.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 175));
                    currentMarker.Shape = v;
                    MainMap.Markers.Add(currentMarker);
                    _StopPoints.Add(point, currentMarker);
                }
            }
        }

        private void AddStopPoints(IList<OSMStopPoint> newPoints)
        {
            lock (_StopPoints1)
            {
                foreach (OSMStopPoint newStopPoint in newPoints)
                {
                    if (!_StopPoints1.ContainsKey(newStopPoint.ID))
                    {
                        _StopPoints1.Add(newStopPoint.ID, newStopPoint);
                    }
                }
            }

        }

        void loadStops_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //        	AddStopPoints(CashHelper.Cash.ReadStops(MainMap.ViewArea));
            using (Dispatcher.DisableProcessing())
            {
                lock (_StopPoints1)
                {
                    if (MainMap.Zoom >= ShowStopMarkersMinLevel && _DrawingStopPoints != null)
                    {
                        DrawStops(_DrawingStopPoints);
                        _DrawingStopPoints = null;
                        _DrawedRects.Add(_DrawingRect);
                    }
                }
            }
        }

        private int _ShowTooltipMinLevel = 10;
        public int ShowTooltipMinLevel
        {
            get { return _ShowTooltipMinLevel; }
            set { _ShowTooltipMinLevel = value; }
        }


        private int _ShowStopMarkersMinLevel = 26;
        public int ShowStopMarkersMinLevel
        {
            get { return _ShowStopMarkersMinLevel; }
            set { _ShowStopMarkersMinLevel = value; }
        }

        IDictionary<Int64, OSMStopPoint> _StopPoints1 = new SortedDictionary<Int64, OSMStopPoint>();
        private void ZoomChanged()
        {
            try
            {
                if (MainMap.Zoom >= ShowTooltipMinLevel)
                {
                    if (!IsViewAreaDrawed(MainMap.ViewArea))
                    {
                        //             IList<OSMStopPoint> stopPoints = CashHelper.Cash.ReadStops(MainMap.ViewArea);
                        //             DrawStops(stopPoints);
                        //             AddStopPoints(CashHelper.Cash.ReadStops(MainMap.ViewArea));
                        /*              DrawStops(stopPoints);
                                          foreach (GMapMarker marker in _StopPoints.Values)
                                          {
                                              StopVisual v = (StopVisual)marker.Shape;
                                              if(((OSMStopPoint)v.Tag).StopType != (int)StopTypes.StopPoint && MainMap.Zoom > 13)
                                                  v.Width = v.Height = 5;
                                          else
                                          {
                                                  v.Width = v.Height = 2;
                                          }
                                          }
                          */
                        if (!_loadBackground.IsBusy)
                            _loadBackground.RunWorkerAsync();
                    }
                    else
                        if (MainMap.Zoom >= ShowStopMarkersMinLevel && _StopPoints.Count == 0)
                        DrawStops(_StopPoints1.Values);
                }
                else
                {
                    _StopPoints1.Clear();
                    _DrawedRects.Clear();
                }
                if (MainMap.Zoom < ShowStopMarkersMinLevel)
                {
                    foreach (GMapMarker marker in _StopPoints.Values)
                        MainMap.Markers.Remove(marker);
                    _StopPoints.Clear();
                }
                if (busMarkers != null)
                {
                    foreach (BusMarker busMarker in busMarkers.Values)
                    {
                        busMarker.ResizeByZoom(MainMap.Zoom);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void StopMarker_ShowStopInfo(object sender, ShowStopInfoEventArgs e)
        {
            if (e != null && e.StopPoint != null)
            {
                GPoint gPoint = MainMap.FromLatLngToLocal(new PointLatLng(e.StopPoint.Latitude, e.StopPoint.Longtitude));
                if (_StopTooltype != null)
                    HideStopPopup();
                ShowStopProperties(e.StopPoint, new Point(gPoint.X, gPoint.Y));
            }
        }

        void StopMarker_HideStopInfo(object sender, ShowStopInfoEventArgs e)
        {
            HideStopPopup();
        }

        OSMRoutePanel _RoutePanel;
        protected void OnRouteButtonClick(object sender, RoutedEventArgs e)
        {
            OSMOTRouteLite routeLite = ((Button)sender).Tag as OSMOTRouteLite;
            if (routeLite != null)
            {
                OSMOTRoute route = CashHelper.Cash.ReadRoute(routeLite.id);
                if (_RoutePanel != null)
                {
                    myCanvas.Children.Remove(_RoutePanel);
                    _RoutePanel = null;
                }
                _RoutePanel = new OSMRoutePanel();
                Canvas.SetTop(_RoutePanel, 2);
                Canvas.SetLeft(_RoutePanel, this.ActualWidth - 200);
                Canvas.SetZIndex(_RoutePanel, 2);
                _RoutePanel.Height = this.Height - 40;
                myCanvas.Children.Add(_RoutePanel);

                _RoutePanel.SetValues(route);
            }
        }

        private OSMOTRoute _Route;
        void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }

        #region -- performance test--
        public RenderTargetBitmap ToImageSource(FrameworkElement obj)
        {
            // Save current canvas transform
            Transform transform = obj.LayoutTransform;
            obj.LayoutTransform = null;

            // fix margin offset as well
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0, margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            System.Windows.Size size = new System.Windows.Size(obj.Width, obj.Height);

            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(obj);

            if (bmp.CanFreeze)
            {
                bmp.Freeze();
            }

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;

            return bmp;
        }

        double NextDouble(Random rng, double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }

        Random r = new Random();

        int tt = 0;
        void timer_Tick(object sender, EventArgs e)
        {
            var pos = new PointLatLng(NextDouble(r, MainMap.ViewArea.Top, MainMap.ViewArea.Bottom), NextDouble(r, MainMap.ViewArea.Left, MainMap.ViewArea.Right));
            GMapMarker m = new GMapMarker(pos);
            {
                var s = new Test((tt++).ToString());

                var image = new Image();
                {
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.LowQuality);
                    image.Stretch = Stretch.None;
                    image.Opacity = s.Opacity;

                    image.MouseEnter += new System.Windows.Input.MouseEventHandler(image_MouseEnter);
                    image.MouseLeave += new System.Windows.Input.MouseEventHandler(image_MouseLeave);

                    image.Source = ToImageSource(s);
                }

                m.Shape = image;

                m.Offset = new System.Windows.Point(-s.Width, -s.Height);
            }
            MainMap.Markers.Add(m);

            if (tt >= 333)
            {
                timer.Stop();
                tt = 0;
            }
        }

        void image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image img = sender as Image;
            img.RenderTransform = null;
        }

        void image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image img = sender as Image;
            img.RenderTransform = new ScaleTransform(1.2, 1.2, 12.5, 12.5);
        }

        DispatcherTimer timer = new DispatcherTimer();
        #endregion

        #region -- transport demo --
        BackgroundWorker transport = new BackgroundWorker();
        BackgroundWorker _loadBackground = new BackgroundWorker();

        readonly List<VehicleData> trolleybus = new List<VehicleData>();
        readonly Dictionary<int, GMapMarker> trolleybusMarkers = new Dictionary<int, GMapMarker>();

        readonly List<VehicleData> bus = new List<VehicleData>();
        readonly Dictionary<int, BusMarker> busMarkers = new Dictionary<int, BusMarker>();

        bool firstLoadTrasport = true;



        void transport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            using (Dispatcher.DisableProcessing())
            {
                /*
                 * lock(trolleybus)
                            {
                               foreach(VehicleData d in trolleybus)
                               {
                                  GMapMarker marker;

                                  if(!trolleybusMarkers.TryGetValue(d.Id, out marker))
                                  {
                                     marker = new GMapMarker(new PointLatLng(d.Lat, d.Lng));
                                     marker.Tag = d.Id;
                                     marker.Shape = new CircleVisual(marker, Brushes.Red);

                                     trolleybusMarkers[d.Id] = marker;
                                     MainMap.Markers.Add(marker);
                                  }
                                  else
                                  {
                                     marker.Position = new PointLatLng(d.Lat, d.Lng);
                                     var shape = (marker.Shape as CircleVisual);
                                     {
                                        shape.Text = d.Line;
                                        shape.Angle = d.Bearing;
                                        shape.Tooltip.SetValues("TrolleyBus", d);

                                        if(shape.IsChanged)
                                        {
                                           shape.UpdateVisual(false);
                                        }
                                     }
                                  }
                               }
                            }
                */


                lock (bus)
                {
                    foreach (VehicleData d in bus)
                    {
                        DateTime lastTime;
                        //if (DateTime.TryParse(d.Time, CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.AllowWhiteSpaces, out lastTime))
                        //{
                        lastTime = d.Time;
                        if (DateTime.Now.AddMinutes(-5) <= lastTime)
                        {
                            BusMarker marker;

                            if (!busMarkers.TryGetValue(d.Id, out marker))
                            {
                                marker = new BusMarker(new PointLatLng(d.Latitude, d.Longitude));
                                marker.Tag = d.Id;
                                marker.ID = d.Id;
                                var v = new PTVehicleVisual(marker, new SolidColorBrush(Color.FromRgb(d.Red, d.Green, d.Blue)), d);
                                {
                                    v.Stroke = new Pen(Brushes.Gray, 2.0);
                                }
                                marker.Shape = v;
                                marker.ResizeByZoom(MainMap.Zoom);
                                busMarkers[d.Id] = marker;
                                MainMap.Markers.Add(marker);
                            }
                            marker.Position = new PointLatLng(d.Latitude, d.Longitude);
                            marker.TimeStamp = lastTime;
                            var shape = (marker.Shape as PTVehicleVisual);
                            {
                                shape.VehicleData = d;
                                if (DateTime.Now.AddMinutes(-2) > lastTime)
                                {
                                    shape.Background.Opacity = 0.3;
                                }
                                else
                                {
                                    shape.Background.Opacity = 1;
                                }
                            }
                            //shape.Tooltip.SetValues(d);

                            if (shape.IsChanged)
                            {
                                shape.UpdateVisual(false);
                            }
                        }
                        //}
                    }
                    BusMarker[] tempBusMarkers = busMarkers.Values.ToArray();
                    bool isRemoved = false;
                    for (int i = 0; i < tempBusMarkers.Length; i++)
                    {
                        BusMarker marker = tempBusMarkers[i];
                        if (DateTime.Now.AddMinutes(-5) > marker.TimeStamp)
                        {
                            busMarkers.Remove(marker.ID);
                            MainMap.Markers.Remove(marker);
                            isRemoved = true;
                        }
                        else
                            if (DateTime.Now.AddMinutes(-2) > marker.TimeStamp)
                        {
                            (marker.Shape as CircleVisual).Background.Opacity = 0.3;
                        }
                    }
                    if (isRemoved)
                        MainMap.InvalidateVisual();
                }

                if (firstLoadTrasport)
                {
                    firstLoadTrasport = false;
                    MainMap.InvalidateVisual();
                }
            }
        }

        void transport_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!transport.CancellationPending)
            {
                try
                {
                    lock (trolleybus)
                    {
                        Stuff.GetEttuTransportData(OSMOTRouteTypes.Trolleybus, string.Empty, trolleybus);
                    }

                    lock (bus)
                    {
                        //                  Stuff.GetVilniusTransportData(TransportType.Bus, string.Empty, bus);
                        Stuff.GetEttuTransportData(OSMOTRouteTypes.Tramway, string.Empty, bus);
                    }

                    transport.ReportProgress(100);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("transport_DoWork: " + ex.ToString());
                }
                Thread.Sleep(2000);
            }
            trolleybusMarkers.Clear();
            busMarkers.Clear();
        }

        #endregion

        // add objects and zone around them
        void AddDemoZone(double areaRadius, PointLatLng center, List<PointAndInfo> objects)
        {
            var objectsInArea = from p in objects
                                where MainMap.MapProvider.Projection.GetDistance(center, p.Point) <= areaRadius
                                select new
                                {
                                    Obj = p,
                                    Dist = MainMap.MapProvider.Projection.GetDistance(center, p.Point)
                                };
            if (objectsInArea.Any())
            {
                var maxDistObject = (from p in objectsInArea
                                     orderby p.Dist descending
                                     select p).First();

                // add objects to zone
                foreach (var o in objectsInArea)
                {
                    GMapMarker it = new GMapMarker(o.Obj.Point);
                    {
                        it.ZIndex = 55;
                        var s = new CustomMarkerDemo(this, it, o.Obj.Info + ", distance from center: " + o.Dist + "km.");
                        it.Shape = s;
                    }

                    MainMap.Markers.Add(it);
                }

                // add zone circle
                //if(false)
                {
                    GMapMarker it = new GMapMarker(center);
                    it.ZIndex = -1;

                    Circle c = new Circle();
                    c.Center = center;
                    c.Bound = maxDistObject.Obj.Point;
                    c.Tag = it;
                    c.IsHitTestVisible = false;

                    UpdateCircle(c);
                    Circles.Add(it);

                    it.Shape = c;
                    MainMap.Markers.Add(it);
                }
            }
        }

        // calculates circle radius
        void UpdateCircle(Circle c)
        {
            var pxCenter = MainMap.FromLatLngToLocal(c.Center);
            var pxBounds = MainMap.FromLatLngToLocal(c.Bound);

            double a = (double)(pxBounds.X - pxCenter.X);
            double b = (double)(pxBounds.Y - pxCenter.Y);
            var pxCircleRadius = Math.Sqrt(a * a + b * b);

            c.Width = 55 + pxCircleRadius * 2;
            c.Height = 55 + pxCircleRadius * 2;
            (c.Tag as GMapMarker).Offset = new System.Windows.Point(-c.Width / 2, -c.Height / 2);
        }

        void MainMap_OnMapTypeChanged(GMapProvider type)
        {
            sliderZoom.Minimum = MainMap.MinZoom;
            sliderZoom.Maximum = MainMap.MaxZoom;
        }

        void MainMap_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(MainMap);
            currentMarker.Position = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
        }

        // move current marker with left holding
        void MainMap_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            /*        if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                     {
                        System.Windows.Point p = e.GetPosition(MainMap);
                        currentMarker.Position = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
                     }
                     */
            if (MainMap.Zoom >= _ShowTooltipMinLevel && _StopPoints1 != null && e.RightButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                double difference = 3;

                //            difference = difference / (2 << (Convert.ToInt32(MainMap.Zoom) - 12));
                double currentDifference = 100;
                OSMStopPoint bestPoint = null;
                System.Windows.Point p = e.GetPosition(MainMap);
                PointLatLng position = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
                lock (_StopPoints1)
                {
                    foreach (OSMStopPoint stopPoint in _StopPoints1.Values)
                    {
                        GPoint stopWindPoint = MainMap.FromLatLngToLocal(stopPoint.Point);
                        double latVariance = Math.Abs(p.X - stopWindPoint.X);
                        double lngVariance = Math.Abs(p.Y - stopWindPoint.Y);
                        double pointDifference = Math.Sqrt(latVariance * latVariance + lngVariance * lngVariance);
                        if (pointDifference < difference && pointDifference < currentDifference)
                        {
                            bestPoint = stopPoint;
                            currentDifference = pointDifference;
                        }
                    }
                }
                if (bestPoint != null)
                {
                    //         		if(MainMap.Zoom < _ShowStopMarkersMinLevel )
                    ShowStopProperties(bestPoint, p);
                }
                else
                    //                if (MainMap.Zoom < _ShowStopMarkersMinLevel)
                    HideStopPopup();
            }

        }

        private void HideStopPopup()
        {
            if (_StopTooltype != null)
            {
                myCanvas.Children.Remove(_StopTooltype);
                _StopTooltype = null;
            }

        }

        private StopTooltip _StopTooltype;
        public void eh(object sender, MouseButtonEventArgs e)
        {
            e = e;
        }
        public void ShowStopProperties(OSMStopPoint stopPoint, System.Windows.Point p)
        {
            if (_StopTooltype != null)
                return;
            _StopTooltype = new StopTooltip();
            Canvas.SetTop(_StopTooltype, p.Y + 22);
            Canvas.SetLeft(_StopTooltype, p.X + 18);
            Canvas.SetZIndex(_StopTooltype, 20000);

            myCanvas.Children.Insert(0, _StopTooltype);
            _StopTooltype.MouseDoubleClick += new MouseButtonEventHandler(eh);
            _StopTooltype.Focus();
            _StopTooltype.SetValues(stopPoint);
            _StopTooltype.RouteButtonClick += new RoutedEventHandler(OnRouteButtonClick);
        }
        // zoo max & center markers
        private void button13_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ZoomAndCenterMarkers(null);

            /*
            PointAnimation panMap = new PointAnimation();
            panMap.Duration = TimeSpan.FromSeconds(1);
            panMap.From = new Point(MainMap.Position.Lat, MainMap.Position.Lng);
            panMap.To = new Point(0, 0);
            Storyboard.SetTarget(panMap, MainMap);
            Storyboard.SetTargetProperty(panMap, new PropertyPath(GMapControl.MapPointProperty));

            Storyboard panMapStoryBoard = new Storyboard();
            panMapStoryBoard.Children.Add(panMap);
            panMapStoryBoard.Begin(this);
             */
        }

        // tile louading starts
        void MainMap_OnTileLoadStart()
        {
            System.Windows.Forms.MethodInvoker m = delegate ()
            {
                progressBar1.Visibility = Visibility.Visible;
            };

            try
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, m);
            }
            catch
            {
            }
        }

        // tile loading stops
        void MainMap_OnTileLoadComplete(long ElapsedMilliseconds)
        {
            MainMap.ElapsedMilliseconds = ElapsedMilliseconds;

            System.Windows.Forms.MethodInvoker m = delegate ()
            {
                progressBar1.Visibility = Visibility.Hidden;
                groupBox3.Header = "loading, last in " + MainMap.ElapsedMilliseconds + "ms";
            };

            try
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, m);
            }
            catch
            {
            }
        }


        // current location changed
        void MainMap_OnCurrentPositionChanged(PointLatLng point)
        {
            mapgroup.Header = "gmap: " + point;
            ZoomChanged();
        }

        // reload
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ReloadMap();
        }

        // enable current marker
        private void checkBoxCurrentMarker_Checked(object sender, RoutedEventArgs e)
        {
            if (currentMarker != null)
            {
                MainMap.Markers.Add(currentMarker);
            }
        }

        // disable current marker
        private void checkBoxCurrentMarker_Unchecked(object sender, RoutedEventArgs e)
        {
            if (currentMarker != null)
            {
                MainMap.Markers.Remove(currentMarker);
            }
        }

        // enable map dragging
        private void checkBoxDragMap_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.CanDragMap = true;
        }

        // disable map dragging
        private void checkBoxDragMap_Unchecked(object sender, RoutedEventArgs e)
        {
            MainMap.CanDragMap = false;
        }

        // goto!
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double lat = double.Parse(textBoxLat.Text, CultureInfo.InvariantCulture);
                double lng = double.Parse(textBoxLng.Text, CultureInfo.InvariantCulture);

                MainMap.Position = new PointLatLng(lat, lng);
            }
            catch (Exception ex)
            {
                MessageBox.Show("incorrect coordinate format: " + ex.Message);
            }
        }

        // goto by geocoder
        private void textBoxGeo_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GeoCoderStatusCode status = MainMap.SetPositionByKeywords(textBoxGeo.Text);
                if (status != GeoCoderStatusCode.G_GEO_SUCCESS)
                {
                    MessageBox.Show("Google Maps Geocoder can't find: '" + textBoxGeo.Text + "', reason: " + status.ToString(), "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    currentMarker.Position = MainMap.Position;
                }
            }
        }

        // zoom changed
        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // updates circles on map
            foreach (var c in Circles)
            {
                UpdateCircle(c.Shape as Circle);
            }
        }

        // zoom up
        private void czuZoomUp_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = ((int)MainMap.Zoom) + 1;
        }

        // zoom down
        private void czuZoomDown_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = ((int)(MainMap.Zoom + 0.99)) - 1;
        }

        // prefetch
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            RectLatLng area = MainMap.SelectedArea;
            if (!area.IsEmpty)
            {
                for (int i = (int)MainMap.Zoom; i <= MainMap.MaxZoom; i++)
                {
                    MessageBoxResult res = MessageBox.Show("Ready ripp at Zoom = " + i + " ?", "GMap.NET", MessageBoxButton.YesNoCancel);

                    if (res == MessageBoxResult.Yes)
                    {
                        TilePrefetcher obj = new TilePrefetcher();
                        obj.Owner = this;
                        obj.ShowCompleteMessage = true;
                        obj.Start(area, i, MainMap.MapProvider, 100);
                    }
                    else if (res == MessageBoxResult.No)
                    {
                        continue;
                    }
                    else if (res == MessageBoxResult.Cancel)
                    {
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        // access mode
        private void comboBoxMode_DropDownClosed(object sender, EventArgs e)
        {
            MainMap.Manager.Mode = (AccessMode)comboBoxMode.SelectedItem;
            MainMap.ReloadMap();
        }

        // clear cache
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are You sure?", "Clear GMap.NET cache?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                try
                {
                    MainMap.Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, null);
                    MessageBox.Show("Done. Cache is clear.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // export
        private void button6_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ShowExportDialog();
        }

        // import
        private void button5_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ShowImportDialog();
        }

        // use route cache
        private void checkBoxCacheRoute_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.Manager.UseRouteCache = checkBoxCacheRoute.IsChecked.Value;
        }

        // use geocoding cahce
        private void checkBoxGeoCache_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.Manager.UseGeocoderCache = checkBoxGeoCache.IsChecked.Value;
            MainMap.Manager.UsePlacemarkCache = MainMap.Manager.UseGeocoderCache;
        }

        // save currnt view
        private void button7_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImageSource img = MainMap.ToImageSource();
                PngBitmapEncoder en = new PngBitmapEncoder();
                en.Frames.Add(BitmapFrame.Create(img as BitmapSource));

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "GMap.NET Image"; // Default file name
                dlg.DefaultExt = ".png"; // Default file extension
                dlg.Filter = "Image (.png)|*.png"; // Filter files by extension
                dlg.AddExtension = true;
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                // Show save file dialog box
                bool? result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    // Save document
                    string filename = dlg.FileName;

                    using (System.IO.Stream st = System.IO.File.OpenWrite(filename))
                    {
                        en.Save(st);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // clear all markers
        private void button10_Click(object sender, RoutedEventArgs e)
        {
            var clear = MainMap.Markers.Where(p => p != null && p != currentMarker);
            if (clear != null)
            {
                for (int i = 0; i < clear.Count(); i++)
                {
                    MainMap.Markers.Remove(clear.ElementAt(i));
                    i--;
                }
            }

            if (radioButtonPerformance.IsChecked == true)
            {
                tt = 0;
                if (!timer.IsEnabled)
                {
                    timer.Start();
                }
            }
        }

        // add marker
        private void button8_Click(object sender, RoutedEventArgs e)
        {
            GMapMarker m = new GMapMarker(currentMarker.Position);
            {
                Placemark? p = null;
                if (checkBoxPlace.IsChecked.Value)
                {
                    GeoCoderStatusCode status;
                    var plret = GMapProviders.GoogleMap.GetPlacemark(currentMarker.Position, out status);
                    if (status == GeoCoderStatusCode.G_GEO_SUCCESS && plret != null)
                    {
                        p = plret;
                    }
                }

                string ToolTipText;
                if (p != null)
                {
                    ToolTipText = p.Value.Address;
                }
                else
                {
                    ToolTipText = currentMarker.Position.ToString();
                }

                m.Shape = new CustomMarkerDemo(this, m, ToolTipText);
                m.ZIndex = 55;
            }
            MainMap.Markers.Add(m);
        }

        // sets route start
        private void button11_Click(object sender, RoutedEventArgs e)
        {
            start = currentMarker.Position;
        }

        // sets route end
        private void button9_Click(object sender, RoutedEventArgs e)
        {
            end = currentMarker.Position;
        }


        BackgroundWorker _buildRouteBackground;
        private MapRoute _BuildedRoute;
        private string _Source_file;

        private void Do_BuildRoute(object sender, DoWorkEventArgs e)
        {
            _BuildedRoute = null;
            _BuildedRoute = GetRoute(start, end, false, false);
            _buildRouteBackground.ReportProgress(100);
        }

        void BuildRoute_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //        	AddStopPoints(CashHelper.Cash.ReadStops(MainMap.ViewArea));
            using (Dispatcher.DisableProcessing())
            {
                if (_BuildedRoute != null)
                {
                    GMapMarker m1 = new GMapMarker(start);
                    m1.Shape = new CustomMarkerDemo(this, m1, "Start: " + _BuildedRoute.Name);

                    GMapMarker m2 = new GMapMarker(end);
                    m2.Shape = new CustomMarkerDemo(this, m2, "End: " + start.ToString());

                    GMapRoute mRoute = new GMapRoute(_BuildedRoute.Points);
                    //       MapRoute route = rp.GetRoute(start, end, false, false, (int)MainMap.Zoom);
                    //                 {
                    //  mRoute.Route.AddRange(_BuildedRoute.Points);
                    //mRoute.RegenerateRouteShape(MainMap);

                    //mRoute.ZIndex = -1;
                    //                  }

                    MainMap.Markers.Add(m1);
                    MainMap.Markers.Add(m2);
                    MainMap.Markers.Add(mRoute);

                    MainMap.ZoomAndCenterMarkers(null);
                }
            }
        }

        // adds route
        private void button12_Click(object sender, RoutedEventArgs e)
        {
            RoutingProvider rp = MainMap.MapProvider as RoutingProvider;
            if (rp == null)
            {
                rp = GMapProviders.GoogleMap; // use google if provider does not implement routing
            }
            if (_target_data == null)
            {

                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



                // Set filter for file extension and default file extension

                dlg.DefaultExt = ".pbf";

                dlg.Filter = "Карты (.pbf)|*.pbf";



                // Display OpenFileDialog by calling ShowDialog method

                Nullable<bool> result = dlg.ShowDialog();



                // Get the selected file name and display in a TextBox

                if (result != true)
                {
                    return;
                }
                _Source_file = dlg.FileName;

                // Open document
                if (_buildRouteBackground == null)
                {
                    _buildRouteBackground = new BackgroundWorker();
                    _buildRouteBackground.DoWork += new DoWorkEventHandler(Do_BuildRoute);
                    _buildRouteBackground.ProgressChanged += new ProgressChangedEventHandler(BuildRoute_ProgressChanged);
                    _buildRouteBackground.WorkerSupportsCancellation = true;
                    _buildRouteBackground.WorkerReportsProgress = true;
                }


                //              string source_file = "D:\\RU-SVE.osm.pbf";
            }
            _buildRouteBackground.RunWorkerAsync();

            /*
                     MapRoute route = GetRoute(start, end, false, false, (int)MainMap.Zoom);
                     if(route != null)
                     {
                        GMapMarker m1 = new GMapMarker(start);
                        m1.Shape = new CustomMarkerDemo(this, m1, "Start: " + route.Name);

                        GMapMarker m2 = new GMapMarker(end);
                        m2.Shape = new CustomMarkerDemo(this, m2, "End: " + start.ToString());

                        GMapMarker mRoute = new GMapMarker(route.Points[0]);
                        {
                           mRoute.Route.AddRange(route.Points);
                           mRoute.RegenerateRouteShape(MainMap);

                           mRoute.ZIndex = -1;
                        }

                        MainMap.Markers.Add(m1);
                        MainMap.Markers.Add(m2);
                        MainMap.Markers.Add(mRoute);

                        MainMap.ZoomAndCenterMarkers(null);
                     }
             * */
        }

        // enables tile grid view
        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.ShowTileGridLines = true;
        }

        // disables tile grid view
        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            MainMap.ShowTileGridLines = false;
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int offset = 22;

            if (MainMap.IsFocused)
            {
                if (e.Key == Key.Left)
                {
                    MainMap.Offset(-offset, 0);
                }
                else if (e.Key == Key.Right)
                {
                    MainMap.Offset(offset, 0);
                }
                else if (e.Key == Key.Up)
                {
                    MainMap.Offset(0, -offset);
                }
                else if (e.Key == Key.Down)
                {
                    MainMap.Offset(0, offset);
                }
                else if (e.Key == Key.Add)
                {
                    czuZoomUp_Click(null, null);
                }
                else if (e.Key == Key.Subtract)
                {
                    czuZoomDown_Click(null, null);
                }
            }
        }

        private void DrawRoute()
        {
            if (_Route == null)
                return;

            foreach (OSMOTRouteStop routeStop in _Route.Stops)
                foreach (OSMStopPoint stop in routeStop.StopPoints.Values)
                {
                    GMapMarker marker = new GMapMarker(new PointLatLng(stop.Latitude, stop.Longtitude));
                    //marker.Tag = d.Id;

                    var v = new StopVisual(marker, new SolidColorBrush(Color.FromRgb(255, 0, 0)));
                    {
                        v.Stroke = new Pen(Brushes.Gray, 2.0);
                        v.Text = stop.Name;
                        v.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 175));
                    }
                    marker.Shape = v;

                    MainMap.Markers.Add(marker);
                }

            //            GMapRoute mRoute = new GMapRoute(start);

            List<PointLatLng> points = new List<PointLatLng>();
            foreach (OSMWay way in _Route.Ways)
            {
                foreach (OSMPoint point in way.Points)
                {
                    points.Add(new PointLatLng(point.Latitude, point.Longtitude));
                    //   mRoute.Route.Add(new PointLatLng(point.Latitude, point.Longtitude));
                }
            }
            //mRoute.RegenerateRouteShape(MainMap);
            GMapRoute mRoute = new GMapRoute(points);
            mRoute.ZIndex = -1;

            MainMap.Markers.Add(mRoute);
        }

        // set real time demo
        private void realTimeChanged(object sender, RoutedEventArgs e)
        {
            //         MainMap.Markers.Clear();
            //         DrawRoute();
            // start performance test
            if (radioButtonPerformance.IsChecked == true)
            {
                timer.Start();
            }
            else
            {
                // stop performance test
                timer.Stop();
            }

            // start realtime transport tracking demo
            if (radioButtonTransport.IsChecked == true)
            {
                if (!transport.IsBusy)
                {
                    firstLoadTrasport = true;
                    transport.RunWorkerAsync();
                }
            }
            else
            {
                if (transport.IsBusy)
                {
                    transport.CancelAsync();
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                MainMap.Bearing--;
            }
            else if (e.Key == Key.Z)
            {
                MainMap.Bearing++;
            }
        }

        private void MapProviderMenu_IsSelectedChanged(object sender, EventArgs e)
        {
            e = e;
        }

        SimpleWeighedDataGraphProcessingTarget _target_data;
        MemoryRouterDataSource<SimpleWeighedEdge> _osm_data;

        private MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode)
        {
            // keeps a memory-efficient version of the osm-tags.
            OsmTagsIndex tags_index = new OsmTagsIndex();

            // creates a routing interpreter. (used to translate osm-tags into a routable network)
            OsmRoutingInterpreter interpreter = new OsmRoutingInterpreter();

            // create a routing datasource, keeps all processed osm routing data.

            // load data into this routing datasource.
            //Stream osm_xml_data = new FileInfo(@"d:\svr.osm").OpenRead(); // for example moscow!
            //using (osm_xml_data)
            if (_target_data == null)
            {
                if (_Source_file == null)
                    return null;
                _osm_data =
                  new MemoryRouterDataSource<SimpleWeighedEdge>(tags_index);

                /*
                 *          Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



                             // Set filter for file extension and default file extension

                             dlg.DefaultExt = ".pbf";

                             dlg.Filter = "Карты (.pbf)|*.pbf";



                             // Display OpenFileDialog by calling ShowDialog method

                             Nullable<bool> result = dlg.ShowDialog();



                             // Get the selected file name and display in a TextBox

                             if (result != true)
                             {

                                 // Open document
                                 return null;

                             }
               //              string source_file = "D:\\RU-SVE.osm.pbf";

                             string source_file = dlg.FileName;
               */
                PBFDataProcessorSource data_processor_source = new PBFDataProcessorSource(new
                FileInfo(_Source_file).OpenRead());

                //              XmlDataProcessorSource data_processor_source = new XmlDataProcessorSource(osm_xml_data);
                _target_data = new SimpleWeighedDataGraphProcessingTarget(_osm_data, interpreter, _osm_data.TagsIndex, VehicleEnum.Car);

                // replace this with PBFdataProcessSource when having downloaded a PBF file.
                //XmlDataProcessorSource data_processor_source = new
                //XmlDataProcessorSource(osm_xml_data);
                //SQLServerSimpleSchemaDataProcessorTarget sqlServerSource = new SQLServerSimpleSchemaDataProcessorTarget("Data Source=ND-SQL-TEST;Initial Catalog=OSMData;Integrated Security=True;Pooling=False;", true);
                //sqlServerSource.RegisterSource(data_processor_source);
                //sqlServerSource.Pull();
                // pre-process the data.
                DataProcessorFilterSort sorter = new DataProcessorFilterSort();
                sorter.RegisterSource(data_processor_source);
//                sorter.RegisterSource(_OsmDataSource);
                _target_data.RegisterSource(sorter);
                _target_data.Pull();
            }



            // create the router object: there all routing functions are available.
            Router<SimpleWeighedEdge> router = new Router<SimpleWeighedEdge>(_osm_data, interpreter,
            new DykstraRoutingLive(_osm_data.TagsIndex));
            RouterPoint point1 = router.Resolve(VehicleEnum.Car, new GeoCoordinate(start.Lat, start.Lng)); // 56.7850348, 60.54333691));
            RouterPoint point2 = router.Resolve(VehicleEnum.Car, new GeoCoordinate(end.Lat, end.Lng)); // 56.7663019, 60.5499044));

            // calculate route.
            //          MainMap.Markers.Clear();
            OsmSharpRoute route = router.Calculate(VehicleEnum.Car, point1, point2);
            IEnumerable<PointLatLng> points = from routePoint in route.Entries select new PointLatLng(routePoint.Latitude, routePoint.Longitude);// new List<PointLatLng>(route.Entries.Count());
            foreach (RoutePointEntry point in router.Points.Keys)
            {
                //              PointLatLng lt = new PointLatLng(point.Latitude, point.Longitude);
                //              SetMarker(point.Latitude, point.Longitude, router.Points[point]);
                //GMapMarker m2 = new GMapMarker(new PointLatLng(point.Latitude, point.Longitude));
                //m2.Shape = new CustomMarkerDemo(this, m2, router.Points[point]);
                //MainMap.Markers.Add(m2);
            }
            return new MapRoute(points, "Маршрут");
        }

        private MapRoute GetRouteDB(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode, int Zoom)
        {
            // keeps a memory-efficient version of the osm-tags.
            OsmTagsIndex tags_index = new OsmTagsIndex();

            // creates a routing interpreter. (used to translate osm-tags into a routable network)
            OsmRoutingInterpreter interpreter = new OsmRoutingInterpreter();

            // create a routing datasource, keeps all processed osm routing data.
            DynamicGraphRouterDataSource<SimpleWeighedEdge> osm_data =
            new DynamicGraphRouterDataSource<SimpleWeighedEdge>(tags_index);
            osm_data.SupportsProfile(VehicleEnum.Car);
            // load data into this routing datasource.
            /*          Stream osm_xml_data = new FileInfo(@"d:\svr.osm").OpenRead(); // for example moscow!
                      using (osm_xml_data)
                      {

                          SimpleWeighedDataGraphProcessingTarget target_data = new
                          SimpleWeighedDataGraphProcessingTarget(
                          osm_data, interpreter, osm_data.TagsIndex, VehicleEnum.Car);

                          // replace this with PBFdataProcessSource when having downloaded a PBF file.
                          XmlDataProcessorSource data_processor_source = new
                          XmlDataProcessorSource(osm_xml_data);*/
            /*
             * string source_file = "D:\\RU-SVE.osm.pbf";
            PBFDataProcessorSource data_processor_source = new PBFDataProcessorSource(new
            FileInfo(source_file).OpenRead());
                           SQLServerSimpleSchemaDataProcessorTarget sqlServerSource = new SQLServerSimpleSchemaDataProcessorTarget("Data Source=ND-SQL-TEST;Initial Catalog=OSMData;Integrated Security=True;Pooling=False;", true);
                          sqlServerSource.RegisterSource(data_processor_source);
                          sqlServerSource.Pull();
                    //  }*/

            SQLServerSimpleSchemaSource source =
            new SQLServerSimpleSchemaSource(
            "Data Source=ND-SQL-TEST;Initial Catalog=OSMData;Integrated Security=True;Pooling=False;");
            OsmSourceRouterDataSource routing_data = new OsmSourceRouterDataSource(
            interpreter, tags_index, source, VehicleEnum.Car);

            // create the router object: there all routing functions are available.
            IRouter<RouterPoint> router = new Router<PreProcessedEdge>(routing_data, interpreter,
            new DykstraRoutingPreProcessed(osm_data.TagsIndex));
            RouterPoint point1 = router.Resolve(VehicleEnum.Car, new GeoCoordinate(start.Lat, start.Lng)); // 56.7850348, 60.54333691));
            SetMarker(point1, "Начало основного маршрута");
            //         point1 = router.Resolve(VehicleEnum.Car, new GeoCoordinate(point1.Location.Latitude, point1.Location.Longitude)); // 56.7850348, 60.54333691));
            RouterPoint point2 = router.Resolve(VehicleEnum.Car, new GeoCoordinate(end.Lat, end.Lng)); // 56.7663019, 60.5499044));
            SetMarker(point2, "Конец основного маршрута");

            RouterPoint point1_1 = router.Resolve(VehicleEnum.Car, new GeoCoordinate(start.Lat, start.Lng)); // 56.7850348, 60.54333691));
            SetMarker(point1_1, "Начало рулёжки");
            //         point1 = router.Resolve(VehicleEnum.Car, new GeoCoordinate(point1.Location.Latitude, point1.Location.Longitude)); // 56.7850348, 60.54333691));
            RouterPoint point1_2 = router.Resolve(VehicleEnum.Car, new GeoCoordinate(end.Lat, end.Lng)); // 56.7663019, 60.5499044));
            SetMarker(point1_2, "Конец рулёжки");
            // calculate route.
            OsmSharpRoute route1 = router.Calculate(VehicleEnum.Car, point1_1, point1);
            OsmSharpRoute route = router.Calculate(VehicleEnum.Car, point1, point2);
            List<PointLatLng> points = new List<PointLatLng>();
            int i = 0;
            foreach (RoutePointEntry routePoint in route1.Entries)
            {
                if (routePoint != null)
                {
                    points.Add(new PointLatLng(routePoint.Latitude, routePoint.Longitude));
                    SetMarker(routePoint.Latitude, routePoint.Longitude, (++i).ToString());
                }
            }
            foreach (RoutePointEntry routePoint in route.Entries)
            {
                if (routePoint != null)
                {
                    points.Add(new PointLatLng(routePoint.Latitude, routePoint.Longitude));
                    SetMarker(routePoint.Latitude, routePoint.Longitude, (++i).ToString());
                }
            }
            //          IEnumerable<PointLatLng> points = from routePoint in route.Entries select routePoint == null ? null : new PointLatLng(routePoint.Latitude, routePoint.Longitude);// new List<PointLatLng>(route.Entries.Count());
            return new MapRoute(points, "Маршрут");
        }

        private void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                //               string requestTest = CashedMessageSerializer.PostMessageSerializer.PrepareRequest(message.Message, null);
                WebRequest request = WebRequest.Create("http://track.t1604.ru/api/data.php");
                request.Method = "POST"; // для отправки используется метод Post
                                         // данные для отправки
                                         // преобразуем данные в массив байтов
                                         //                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(requestTest);
                                         // устанавливаем тип содержимого - параметр ContentType
                request.ContentType = "application/x-www-form-urlencoded";
                string strRequest = "Start_Time=16.06.2019 11.38.52&Stop_Time=16.06.2019 23.58.52&Vehicle=UAZ501";
                byte[] serializedMessage = Encoding.UTF8.GetBytes(strRequest);
                // Устанавливаем заголовок Content-Length запроса - свойство ContentLength
                request.ContentLength = serializedMessage.Length;
                request.Timeout = 5000;
                //записываем данные в поток запроса
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(serializedMessage, 0, serializedMessage.Length);
                    //                   request.ContentLength = dataStream.Length;
                }


                WebResponse response = request.GetResponse();
                string resp = "0";
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        resp = reader.ReadToEnd();
                    }
                }                //byte[] response = _Client.UploadValues(ServerAddress, "POST", values);

            }
            catch(Exception ex)
            {

            }
        }    

        private void SetMarker(RouterPoint point, string title)
        {
            SetMarker(point.Location.Latitude, point.Location.Longitude, title + ": " + point.ToString());
        }

        private void SetMarker(double lat, double lng, string title)
        {
            PointLatLng pointLatLng = new PointLatLng();
            pointLatLng.Lat = lat;
            pointLatLng.Lng = lng;
            GMapMarker m1 = new GMapMarker(pointLatLng);
            StopVisual shape = new StopVisual(m1, new SolidColorBrush(Color.FromRgb(0, 0, 255)));
            shape.Width = 2;
            shape.Text = title;
            m1.Shape = shape;

            MainMap.Markers.Add(m1);
        }

        private IList<TrackPointMarker> _TrackPointMarkers = new List<TrackPointMarker>();

        private void RibbonButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".gpx";
            dlg.Filter = "Трек (.gpx)|*.gpx";
            Nullable<bool> result = dlg.ShowDialog();
            if (result != true)
            {
                return;
            }
            string trackFileName = dlg.FileName;

            GpxDriver gpxDriver = new GpxDriver(trackFileName); // @"C:\Users\rys\Downloads\2018-11-30_11-18_Fri.gpx");
            Track track = gpxDriver.Parse();
            if (_TrackStartMarker != null)
                MainMap.Markers.Remove(_TrackStartMarker);
            if (_TrackLastMarker != null)
                MainMap.Markers.Remove(_TrackLastMarker);
            if (_TrackMarker != null)
                MainMap.Markers.Remove(_TrackMarker);
            if (_SelectedTrackMarker != null)
                MainMap.Markers.Remove(_SelectedTrackMarker);
            foreach(TrackPointMarker trackPointMarker in _TrackPointMarkers)
            {
                MainMap.Markers.Remove(trackPointMarker);
            }
            _TrackPointMarkers.Clear();
            _IsTrackSelecting = false;
            _FirstPoint = _LastPoint = null;
            _Track = track;
            if (track != null)
            {
                foreach (IList<TrackPoint> segment in track.Segments)
                {
                    if (segment.Count > 0)
                    {
                        _TrackStartMarker = new GMapMarker(new PointLatLng(segment[0].Latitude, segment[0].Longitude));
                        _TrackStartMarker.Shape = new CustomMarkerDemo(this, _TrackStartMarker, "Start: ");

                        _TrackLastMarker = new GMapMarker(new PointLatLng(segment.Last().Latitude, segment.Last().Longitude));
                        _TrackLastMarker.Shape = new CustomMarkerDemo(this, _TrackLastMarker, "End: ");
                        List<PointLatLng> points = new List<PointLatLng>(segment.Count);
                        for (int i = 0; i < segment.Count; i++)
                        {
                            if (_FirstPoint == null)
                                _FirstPoint = segment[i];
                            _LastPoint = segment[i];
                            PointLatLng pointLatLng = new PointLatLng(segment[i].Latitude, segment[i].Longitude);
                            points.Add(pointLatLng);
                            TrackPointMarker trackPointMarker = new TrackPointMarker(pointLatLng);
                            trackPointMarker.ID = i;
                            TrackPointVisual v = new TrackPointVisual(trackPointMarker, new SolidColorBrush(Color.FromRgb(0, 0, 0)), segment[i], track);
                            {
                                v.Stroke = new Pen(Brushes.Gray, 2.0);
                                v.OnSelectStartTrackPoint += new SelectStartPointDelegate(this.TrackPoint_SelectStartPoint);
                                v.OnSelectLastTrackPoint += new SelectStartPointDelegate(this.TrackPoint_SelectLastPoint);
                            }
                            trackPointMarker.Shape = v;
                            trackPointMarker.ResizeByZoom(MainMap.Zoom);
                            MainMap.Markers.Add(trackPointMarker);
                            _TrackPointMarkers.Add(trackPointMarker);
                        }
                        _TrackMarker = new GMapRoute(points);
                        _SelectedTrackMarker = new GMapRoute(points);
                        //_SelectedTrackMarker.Shap
                        //       MapRoute route = rp.GetRoute(start, end, false, false, (int)MainMap.Zoom);
                        //                 {
                        //  mRoute.Route.AddRange(_BuildedRoute.Points);
                        //mRoute.RegenerateRouteShape(MainMap);

                        //mRoute.ZIndex = -1;
                        //                  }

                        MainMap.Markers.Add(_TrackStartMarker);
                        MainMap.Markers.Add(_TrackLastMarker);
//                        MainMap.Markers.Add(_TrackMarker);
                        MainMap.Markers.Add(_SelectedTrackMarker);

                        MainMap.ZoomAndCenterMarkers(null);
                    }



                }
            }
            ShowSelectedTrack();

        }
        private GMapMarker _TrackStartMarker;

        private GMapMarker _TrackLastMarker;

        private GMapRoute _TrackMarker;

        private GMapRoute _SelectedTrackMarker;

        private Track _Track;

        private TrackPoint _FirstPoint, _LastPoint;

        private bool _IsTrackSelecting = false;

        protected virtual List<PointLatLng> FormTrackPointList(Track track, TrackPoint firstPoint, TrackPoint lastPoint)
        {
            bool isFirstFound = false;

            if (track == null)
                return null;
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (IList<TrackPoint> segment in track.Segments)
            {
                if (segment.Count > 0)
                {
                    for (int i = 0; i < segment.Count; i++)
                    {
                        if (firstPoint == null)
                        {
                            firstPoint = segment[i];
                            isFirstFound = true;
                        }
                        if (_FirstPoint == segment[i])
                            isFirstFound = true;
                        if ((lastPoint != null && lastPoint == segment[i]) && !isFirstFound)
                        {
                            return points;
                        }
                        if (isFirstFound)
                        {
                            if (lastPoint == null || lastPoint != segment[i])
                            {
                                PointLatLng pointLatLng = new PointLatLng(segment[i].Latitude, segment[i].Longitude);
                                points.Add(pointLatLng);
                            }
                            if (lastPoint != null && lastPoint == segment[i])
                            {
                                PointLatLng pointLatLng = new PointLatLng(segment[i].Latitude, segment[i].Longitude);
                                points.Add(pointLatLng);
                                return points;
                            }
                        }
                    }
                }
            }
            return points;
        }

        protected virtual string FormStringFromTrack(Track track, TrackPoint firstPoint, TrackPoint lastPoint)
        {
            bool isFirstFound = false;

            if (track == null)
                return null;
            string output = "Время,Дистанция,Скорость";
            foreach (IList<TrackPoint> segment in track.Segments)
            {
                if (segment.Count > 0)
                {
                    for (int i = 0; i < segment.Count; i++)
                    {
                        if (firstPoint == null)
                        {
                            firstPoint = segment[i];
                            isFirstFound = true;
                        }
                        if (_FirstPoint == segment[i])
                            isFirstFound = true;
                        if ((lastPoint != null && lastPoint == segment[i]) && !isFirstFound)
                        {
                            return output;
                        }
                        if (isFirstFound)
                        {
                            if (lastPoint == null || lastPoint != segment[i])
                            {
                                output += String.Format("\n{0}, {1}, {2}", segment[i].Time.Subtract(firstPoint.Time).TotalSeconds, (segment[i].DistanceFromStart - firstPoint.DistanceFromStart).Value, segment[i].Speed);
                            }
                            if (lastPoint != null && lastPoint == segment[i])
                            {
                    output += String.Format("\n{0}, {1}, {2}", segment[i].Time.Subtract(firstPoint.Time).TotalSeconds, (segment[i].DistanceFromStart - firstPoint.DistanceFromStart).Value, segment[i].Speed);
                    return output;
                            }
                        }
                    }
                }
            }
            return output;
        }

        TrackPointVisual _FirstTrackPointVisual;

        protected virtual void ShowSelectedTrack()
        {
            groupBox4.Visibility = Visibility.Collapsed;
            groupBox5.Visibility = Visibility.Collapsed;
            markers.Visibility = Visibility.Collapsed;
            routes.Visibility = Visibility.Collapsed;
            tpiTrackPartInfo.SetValues(_LastPoint, _Track);
            tpiTrackPartInfo.Visibility = Visibility.Visible;
            if (_SelectedTrackMarker == null || _Track == null || _FirstPoint == null || _LastPoint == null)
                return;
            MainMap.Markers.Remove(_SelectedTrackMarker);
            _SelectedTrackMarker.Points = FormTrackPointList(_Track, _FirstPoint, _LastPoint);
            MainMap.Markers.Add(_SelectedTrackMarker);
           //          _SelectedTrackMarker.Shape.InvalidateVisual();
           _TrackStartMarker.Position = new PointLatLng(_FirstPoint.Latitude, _FirstPoint.Longitude);
            _TrackLastMarker.Position = new PointLatLng(_LastPoint.Latitude, _LastPoint.Longitude);

        }

        protected virtual void TrackPoint_SelectStartPoint(object sender, SelectStartPointArgs args)
        {
            /*            if (sender != _FirstTrackPointVisual)
                        {
                            _FirstTrackPointVisual = sender as ;
                            if (_FirstTrackPointVisual != null)
                                _FirstTrackPointVisual.SetAsFirst(false);
                            args.Track.FirstPosition = args.TrackPoint;
                        }

                */
            _IsTrackSelecting = true;
            args.Track.FirstPosition = args.TrackPoint;
            _FirstPoint = args.TrackPoint;
            ShowSelectedTrack();

        }

        protected virtual void TrackPoint_SelectLastPoint(object sender, SelectStartPointArgs args)
        {
            if (_IsTrackSelecting)
            {
                _LastPoint = args.TrackPoint;
                ShowSelectedTrack();
            }
        }

        private void TrackPartInfo_FixRequest(UIElement sender)
        {
            _IsTrackSelecting = false;
        }
        private IDataSource _OsmDataSource;
        private void RibbonButtonLoadRoads_Click(object sender, RoutedEventArgs e)
        {
            GeoCoordinateBox area = new GeoCoordinateBox(new GeoCoordinate(56.86, 60.58), new GeoCoordinate(56.83, 60.68));
            _OsmDataSource = LoadRoadsJosm.LoadRoutes(area);
        }

        private void tpiTrackPartInfo_SaveRequest(UIElement sender)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "Трек (.csv)|*.csv";
            Nullable<bool> result = dlg.ShowDialog();
            if (result != true)
            {
                return;
            }
            string output = FormStringFromTrack(_Track, _FirstPoint, _LastPoint);
            if (File.Exists(dlg.FileName))
                File.Delete(dlg.FileName);
            File.AppendAllText(dlg.FileName, output);
        }

        protected virtual void InfoPanel_CloseRequest(UIElement sender)
        {
            sender.Visibility = Visibility.Collapsed;
            groupBox4.Visibility = Visibility.Visible;
            groupBox5.Visibility = Visibility.Visible;
            markers.Visibility = Visibility.Visible;
            routes.Visibility = Visibility.Visible;
            _IsTrackSelecting = false;
        }

        DispatcherTimer timerShowCurrentPosition = new DispatcherTimer();
        public IGeoPositionRegistrator _GeoPositionRegistrator = new GeoLocationRegistratorNet();
        protected CircleVisual _CurrentPositionMarker;
        protected Brush _CurrentPositionMarkerBrushInactive = new SolidColorBrush(Color.FromRgb(100, 100, 100));
        protected Brush _CurrentPositionMarkerBrushActive = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        protected TrackPoint _PreviousTrackPoint;
        protected TrackMessageSender _TrackMessageSender;
        protected CashService _TruckStatusCashService;

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_GeoPositionRegistrator != null)
            {
                _GeoPositionRegistrator.Dispose();
                _GeoPositionRegistrator = null;
            }
            if(_TrackMessageSender != null)
            {
                _TrackMessageSender.Dispose();
                _TrackMessageSender = null;
            }
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public void RegisterGeoPosition(object sender, EventArgs e)
        {
            if (_GeoPositionRegistrator == null)
                return;
            if(_GeoPositionRegistrator.GetRegistratorStatus() != GeoPositionRegistratorStatus.Active)
            {
                if (_CurrentPositionMarker != null)
                {
                    _CurrentPositionMarker.Background = _CurrentPositionMarkerBrushInactive;
                }
 //               return;
            }
            TrackPoint trackPoint = _GeoPositionRegistrator.GetCurrentPosition();
            if(_CurrentPositionMarker == null)
            {
                _CurrentPositionMarker = new CircleVisual(new GMapMarker(new PointLatLng(trackPoint.Latitude, trackPoint.Longitude)), _CurrentPositionMarkerBrushActive);
                MainMap.Markers.Add(_CurrentPositionMarker.Marker);
            }
            else
            {
                _CurrentPositionMarker.Marker.Position = new PointLatLng(trackPoint.Latitude, trackPoint.Longitude);
            }
            if (DateTime.Now.Subtract(trackPoint.Time).Seconds > 2)
            {
                _CurrentPositionMarker.Background = _CurrentPositionMarkerBrushInactive;
                _CurrentPositionMarker.UpdateVisual(true);
            }
            else
            {
                _CurrentPositionMarker.Background = _CurrentPositionMarkerBrushActive;
                _CurrentPositionMarker.UpdateVisual(true);
            }
            if (_TrackMessageSender == null)
            {
                TelemetrySettings telemetrySettings = new TelemetrySettings();
                RegistryOperation registryOperation = new RegistryOperation("Rodsoft\\OSM.PT\\Telemetry");
                registryOperation.LoadSettingsAuto(telemetrySettings);
                //telemetrySettings.ServerAddress = "http://localhost:54831/Default.aspx";
                //telemetrySettings.TransmittingPeriod = 1000;
                //telemetrySettings.ServiceType = 0;
                //telemetrySettings.RequestTimeout = 3000;
                //telemetrySettings.CashFolder = "Cash";
                //telemetrySettings.MaximumItemsPerCashFile = 100;
                //telemetrySettings.MaximumSecondsPerCashFile = 100;
                //registryOperation.SaveSettingsAuto(telemetrySettings);
                if (_TruckStatusCashService == null)
                {
                    _TruckStatusCashService = new CashService(telemetrySettings);
                    _TruckStatusCashService.LoadCashedData();
                }
                _TrackMessageSender = new TrackMessageSender(telemetrySettings);
                _TrackMessageSender.CashService = _TruckStatusCashService;
            }
            if (!String.IsNullOrEmpty(_TrackMessageSender.ServerAddress))
            { 
                TrackMessage trackMessage = new TrackMessage() { Time = DateTime.Now, Vehicle = "С513РК95RUS", TrackPoint = new GPSDataMessage(trackPoint) };
                _TrackMessageSender.SendMessage(trackMessage);
            }
//            MainMap.ZoomAndCenterMarkers(null);
        }

    }

    public class MapValidationRule : ValidationRule
   {
      bool UserAcceptedLicenseOnce = false;
      internal MainWindow Window;

      public override ValidationResult Validate(object value, CultureInfo cultureInfo)
      {
         if(!(value is OpenStreetMapProviderBase))
         {
            if(!UserAcceptedLicenseOnce)
            {
               if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "License.txt"))
               {
                  string ctn = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "License.txt");
                  int li = ctn.IndexOf("License");
                  string txt = ctn.Substring(li);

                  var d = new Demo.WindowsPresentation.Windows.Message();
                  d.richTextBox1.Text = txt;

                  if(true == d.ShowDialog())
                  {
                     UserAcceptedLicenseOnce = true;
                     if(Window != null)
                     {
                        Window.Title += " - license accepted by " + Environment.UserName + " at " + DateTime.Now;
                     }
                  }
               }
               else
               {
                  // user deleted License.txt ;}
                  UserAcceptedLicenseOnce = true;
               }
            }

            if(!UserAcceptedLicenseOnce)
            {
               return new ValidationResult(false, "user do not accepted license ;/");
            }
         }

         return new ValidationResult(true, null);
      }
   }
}
