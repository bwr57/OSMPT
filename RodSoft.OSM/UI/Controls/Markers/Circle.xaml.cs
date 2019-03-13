﻿using GMap.NET;
using System.Windows.Controls;

namespace RodSoft.OSM.UI.Controls.Markers
{
    /// <summary>
    /// Interaction logic for Circle.xaml
    /// </summary>
    public partial class Circle : UserControl
   {
      public Circle()
      {
         InitializeComponent();
      }

      public PointLatLng Center;
      public PointLatLng Bound;
   }
}
