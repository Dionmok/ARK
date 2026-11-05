using System;
using System.Collections.Generic;
using System.Windows;

namespace ARK.App
{
    public enum MenuScope
    {
        Shared,
        MainOnly,
        ChartOnly
    }

    public class MenuActionDefinition
    {
        public string Header { get; set; } = "";
        public string ParentMenu { get; set; } = "";
        public MenuScope Scope { get; set; }
        public Action<Window> Action { get; set; } = _ => { };
    }

    public static class MenuRegistry
    {
        public static readonly List<MenuActionDefinition> Items = new()
        {
            new MenuActionDefinition
            {
                Header = "New C_hart",
                ParentMenu = "File",
                Scope = MenuScope.Shared,
                Action = owner => AppActions.OpenNewChart()
            },
            new MenuActionDefinition
            {
                Header = "_Watchlist",
                ParentMenu = "View",
                Scope = MenuScope.Shared,
                Action = owner => AppActions.OpenWatchlist()
            },
                        new MenuActionDefinition
            {
                Header = "_Quoteboard",
                ParentMenu = "View",
                Scope = MenuScope.Shared,
                Action = owner => AppActions.OpenQuoteboard()
            },
            new MenuActionDefinition
            {
                Header = "_Positions",
                ParentMenu = "Trade",
                Scope = MenuScope.Shared,
                Action = owner => AppActions.OpenPositions()
            },
                        new MenuActionDefinition
            {
                Header = "Trade _Book",
                ParentMenu = "Trade",
                Scope = MenuScope.Shared,
                Action = owner => AppActions.OpenTradeBook()
            },
                        new MenuActionDefinition
            {
                Header = "_Global Orders Window",
                ParentMenu = "Trade",
                Scope = MenuScope.Shared,
                Action = owner => AppActions.OpenGlobalOrders()
            },
            new MenuActionDefinition
            {
                Header = "Orders by S_ymbol",
                ParentMenu = "Trade",
                Scope = MenuScope.Shared,
                Action = owner => AppActions.OpenOrdersBySymbol()
            },
            new MenuActionDefinition
            {
                Header = "_Find Symbol...",
                ParentMenu = "File",
                Scope = MenuScope.MainOnly,
                Action = owner => AppActions.OpenFindSymbol(owner)
            },
            new MenuActionDefinition
            {
                Header = "_Open Chartbook...",
                ParentMenu = "Chartbooks",
                Scope = MenuScope.MainOnly,
                Action = owner => AppActions.OpenChartbookManager(owner)
            },
            new MenuActionDefinition
            {
                Header = "_General...",
                ParentMenu = "Main Settings",
                Scope = MenuScope.MainOnly,
                Action = owner => AppActions.OpenGeneralSettings(owner)
            },
            new MenuActionDefinition
            {
                Header = "_Data Feeds...",
                ParentMenu = "Main Settings",
                Scope = MenuScope.MainOnly,
                Action = owner => AppActions.OpenDataFeedsSettings(owner)
            },
            new MenuActionDefinition
            {
                Header = "_Hotkeys...",
                ParentMenu = "Main Settings",
                Scope = MenuScope.MainOnly,
                Action = owner => AppActions.OpenHotkeysSettings(owner)
            },
            new MenuActionDefinition
            {
                Header = "_Appearance...",
                ParentMenu = "Main Settings",
                Scope = MenuScope.MainOnly,
                Action = owner => AppActions.OpenAppearanceSettings(owner)
            },
        };
    }
}
