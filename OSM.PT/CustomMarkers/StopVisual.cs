using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Demo.WindowsForms;
using Demo.WindowsPresentation.Controls;
using GMap.NET.WindowsPresentation;

namespace Demo.WindowsPresentation.CustomMarkers
{
	public class ShowStopInfoEventArgs : EventArgs
	{
		public double X;
		public double Y;
		public OSMStopPoint StopPoint;
		
		public ShowStopInfoEventArgs(OSMStopPoint stopPoint)
		{
			StopPoint = stopPoint;
		}
		
	}
	
	public delegate void ShowStopInfoDelegate (object sender, ShowStopInfoEventArgs e);
	
    public class StopVisual : FrameworkElement
   {
      public readonly Popup Popup = new Popup();
//      public readonly StopTooltip Tooltip = new StopTooltip();
      public readonly GMapMarker Marker;
      public int Size = 4;
      
      private bool _IsClicked = false;
      
      public event ShowStopInfoDelegate ShowStopInfo;
      public event ShowStopInfoDelegate HideStopInfo;
      public event ShowStopInfoDelegate FreezeStopInfo;

      public StopVisual(GMapMarker m, Brush background)
      {
         Marker = m;
         Marker.ZIndex = 100;

         Popup.AllowsTransparency = true;
         Popup.PlacementTarget = this;
         Popup.Placement = PlacementMode.Mouse;
//         Popup.Child = Tooltip;
//         Popup.Child.Opacity = 0.777;

         SizeChanged += new SizeChangedEventHandler(CircleVisual_SizeChanged);
         MouseEnter += new System.Windows.Input.MouseEventHandler(CircleVisual_MouseEnter);
         MouseLeave += new System.Windows.Input.MouseEventHandler(CircleVisual_MouseLeave);
         MouseDown += new  System.Windows.Input.MouseButtonEventHandler(CircleVisual_MouseClick);
         Loaded += new RoutedEventHandler(OnLoaded);

         Text = "?";

         StrokeArrow.EndLineCap = PenLineCap.Triangle;
         StrokeArrow.LineJoin = PenLineJoin.Round;

         RenderTransform = scale;

         Width = Height = Size;
//         FontSize = (Width/1.55);

         Background = background;
         Angle = null;
      }

      void CircleVisual_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         Marker.Offset = new System.Windows.Point(-e.NewSize.Width/2, -e.NewSize.Height/2);
         scale.CenterX = -Marker.Offset.X;
         scale.CenterY = -Marker.Offset.Y;
      }

      void OnLoaded(object sender, RoutedEventArgs e)
      {
         UpdateVisual(true);
      }

      readonly ScaleTransform scale = new ScaleTransform(1, 1);

      void CircleVisual_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if(Popup.IsOpen)
         {
            Popup.IsOpen = false;
         }

         Marker.ZIndex -= 10000;
         Cursor = Cursors.Arrow;

         this.Effect = null;

         scale.ScaleY = 1;
         scale.ScaleX = 1;
//         if(HideStopInfo != null && !_IsClicked)
//         	HideStopInfo(sender, new ShowStopInfoEventArgs((OSMStopPoint) ((StopVisual) sender).Tag));
		 
         _IsClicked = false;
      }

      void CircleVisual_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
      {
      	
         if(!Popup.IsOpen)
         {
            Popup.IsOpen = true;
         }

         Marker.ZIndex += 10000;
         Cursor = Cursors.Hand;

         this.Effect = ShadowEffect;
		 _IsClicked = false;
         scale.ScaleY = 1.5;
         scale.ScaleX = 1.5;
         //if(ShowStopInfo != null)
         //   ShowStopInfo(sender, new ShowStopInfoEventArgs((OSMStopPoint) ((StopVisual) sender).Tag));
         
      }
      void CircleVisual_MouseClick(object sender, System.Windows.Input.MouseEventArgs e)
      {
      	_IsClicked = true;
      	if(FreezeStopInfo != null)
      		FreezeStopInfo(this, new ShowStopInfoEventArgs((OSMStopPoint) ((StopVisual) sender).Tag));
      }

      public DropShadowEffect ShadowEffect;

      static readonly Typeface Font = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
      FormattedText FText;

      private Brush background = Brushes.Blue;
      public Brush Background
      {
         get
         {
            return background;
         }
         set
         {
            if(background != value)
            {
               background = value;
               IsChanged = true;
            }
         }
      }

      private Brush foreground = Brushes.White;
      public Brush Foreground
      {
         get
         {
            return foreground;
         }
         set
         {
            if(foreground != value)
            {
               foreground = value;
               IsChanged = true;

               ForceUpdateText();
            }
         }
      }

      private Pen stroke = new Pen(Brushes.Blue, 2.0);
      public Pen Stroke
      {
         get
         {
            return stroke;
         }
         set
         {
            if(stroke != value)
            {
               stroke = value;
               IsChanged = true;
            }
         }
      }

      private Pen strokeArrow = new Pen(Brushes.Blue, 2.0);
      public Pen StrokeArrow
      {
         get
         {
            return strokeArrow;
         }
         set
         {
            if(strokeArrow != value)
            {
               strokeArrow = value;
               IsChanged = true;
            }
         }
      }

      public double FontSize = 9;

      private double? angle = 0;
      public double? Angle
      {
         get
         {
            return angle;
         }
         set
         {
            if(!Angle.HasValue || !value.HasValue || (Angle.HasValue && value.HasValue && Math.Abs(angle.Value - value.Value) > 11))
            {
               angle = value;
               IsChanged = true;
            }
         }
      }
      public bool IsChanged = true;

      void ForceUpdateText()
      {
         FText = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Font, FontSize, Foreground);
         IsChanged = true;
      }

      string text;
      public string Text
      {
         get
         {
            return text;
         }
         set
         {
            if(text != value)
            {
               text = value;
               ForceUpdateText();
            }
         }
      }

      Visual _child;
      public virtual Visual Child
      {
         get
         {
            return _child;
         }
         set
         {
            if(_child != value)
            {
               if(_child != null)
               {
                  RemoveLogicalChild(_child);
                  RemoveVisualChild(_child);
               }

               if(value != null)
               {
                  AddVisualChild(value);
                  AddLogicalChild(value);
               }

               // cache the new child
               _child = value;

               InvalidateVisual();
            }
         }
      }

      public bool UpdateVisual(bool forceUpdate)
      {
         if(forceUpdate || IsChanged)
         {
            Child = Create();
            IsChanged = false;
            return true;
         }

         return false;
      }

      int countCreate = 0;

      private DrawingVisual Create()
      {
         countCreate++;

         var square = new DrawingVisualFx();

         using(DrawingContext dc = square.RenderOpen())
         {
            dc.DrawEllipse(null, Stroke, new Point(Width/2, Height/2), Width/2 + Stroke.Thickness/2, Height/2 + Stroke.Thickness/2);

            if(Angle.HasValue)
            {
               dc.PushTransform(new RotateTransform(Angle.Value, Width/2, Height/2));
               {
                  PolyLineSegment polySeg = new PolyLineSegment(new Point[] { new Point(Width*0.2, Height*0.3), new Point(Width*0.8, Height*0.3) }, true);
                  PathFigure pathFig = new PathFigure(new Point(Width*0.5, -Height*0.22), new PathSegment[] { polySeg }, true);
                  PathGeometry pathGeo = new PathGeometry(new PathFigure[] { pathFig });
                  dc.DrawGeometry(Brushes.AliceBlue, StrokeArrow, pathGeo);
               }
               dc.Pop();
            }

            dc.DrawEllipse(Background, null, new Point(Width/2, Height/2), Width/2, Height/2);
             if(IsTextVisible)
                dc.DrawText(FText, new Point(Width/2 - FText.Width/2, - FText.Height/2 - 5));
         }

         return square;
      }

      private bool _IsTextVisible = true;

public bool IsTextVisible
{
  get { return _IsTextVisible; }
  set {
      if (_IsTextVisible != value)
      {
          _IsTextVisible = value;
          UpdateVisual(true);
      }

  }
}
  

      #region Necessary Overrides -- Needed by WPF to maintain bookkeeping of our hosted visuals
      protected override int VisualChildrenCount
      {
         get
         {
            return (Child == null ? 0 : 1);
         }
      }

      protected override Visual GetVisualChild(int index)
      {
         return Child;
      }
      #endregion
   }

}
