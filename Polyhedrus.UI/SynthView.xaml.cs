﻿using Polyhedrus.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Polyhedrus.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class SynthView : Window
	{
		private ViewModel VM;

		public SynthView(ViewModel vm)
		{
			VM = vm;
			DataContext = VM;
			InitializeComponent();
		}

		public SynthView(SynthController ctrl)
		{
			VM = new ViewModel(ctrl);
			DataContext = VM;
			InitializeComponent();
		}

		public SynthView()
		{
			InitializeComponent();
		}
	}
}
