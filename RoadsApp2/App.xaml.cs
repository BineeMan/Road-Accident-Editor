﻿namespace RoadsApp2;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
        MainPage.Title = "Конструктор ДТП";
	}

}
