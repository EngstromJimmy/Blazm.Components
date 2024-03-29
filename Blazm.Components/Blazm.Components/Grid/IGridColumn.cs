﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazm.Components
{
    public interface IGridColumn
    {
        string Id
        {
            get;
            set;
        }

        List<Filter> Filters
        {
            get;
            set;
        }

        Task ApplyFilter();


        bool Exportable
        {
            get;
            set;
        }
            
        int Priority
        {
            get;
            set;
        }

        bool ShowAdvancedFilter
        {
            get;
            set;
        }

        string Class
        {
            get;
            set;
        }

        string HeaderClass
        {
            get;
            set;
        }

        bool Visible
        {
            get;
            set;
        }

        string ValueNegativeClass
        {
            get;
            set;
        }

        string ValuePositiveClass
        {
            get;
            set;
        }


        bool HighlightSign
        {
            get;
            set;
        }

        string DataClass
        {
            get;
            set;
        }

        int ClientWidth
        {
            get;
            set;
        }

        string Title
        {
            get;
            set;
        }

        string Width
        {
            get;
            set;
        }
        string Field
        {
            get;
            set;
        }

        string Format
        {
            get;
            set;
        }

        Type Type
        {
            get;
            set;
        }

        bool CanSort
        { 
            get; 
        }

        bool CanFilter
        {
            get;
            set;
        }

        public RenderFragment<object> FooterTemplate
        {
            get;
            set;
        }

        public RenderFragment<object> GroupFooterTemplate
        {
            get;
            set;
        }

        public RenderFragment<object> Template
        {
            get;
            set;
        }
    }
}
