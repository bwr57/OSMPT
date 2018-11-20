/*
 * Сделано в SharpDevelop.
 * Пользователь: Родион
 * Дата: 03/08/2013
 * Время: 03:01
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Demo.WindowsForms;

namespace Demo.WindowsPresentation.Controls
{
	/// <summary>
	/// Interaction logic for OSMRoutePanel.xaml
	/// </summary>
	public partial class OSMRoutePanel : UserControl
	{
		public OSMRoutePanel()
		{
			InitializeComponent();
		}
		public void SetValues(OSMOTRoute route)
		{
			string strRouteType = "";
			switch(route.RouteType)
			{
				case OSMOTRouteTypes.Bus : 
					{
						strRouteType = "Автобус ";
						break;
					}
				case OSMOTRouteTypes.SharedTaxi : 
					{
						strRouteType = "Маршрутное такси ";
						break;
					}
				case OSMOTRouteTypes.Tramway : 
					{
						strRouteType = "Трамвай ";
						break;
					}
				case OSMOTRouteTypes.Trolleybus : 
					{
						strRouteType = "Троллейбус ";
						break;
					}
				case OSMOTRouteTypes.Train : 
					{
						strRouteType = "Поезд ";
						break;
					}
				case OSMOTRouteTypes.Light_rail : 
					{
						strRouteType = "Электричка ";
						break;
					}
				case OSMOTRouteTypes.Subway : 
					{
						strRouteType = "Метро ";
						break;
					}
			}
			lblRef.Text = String.Format("{0} {1}", strRouteType, route.Ref);
			lblName.Text = route.Name;
			lblOperator.Text = route.Operator;
			if(route.Operator != null)
				lblOperator.Visibility= Visibility.Visible;
			lblNetwork.Text = route.Network;
			if(!String.IsNullOrEmpty(route.Network))
				lblNetwork.Visibility= Visibility.Visible;
			foreach(OSMOTRouteStop stop in route.Stops)
			{
				
				Button but = new Button();
				but.HorizontalContentAlignment = HorizontalAlignment.Left;
				but.Content = stop.Name;
				stkForwardStops.Children.Add(but);
			}
		}
	}
}