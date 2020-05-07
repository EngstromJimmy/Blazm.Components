using Microsoft.AspNetCore.Components;
using System;

namespace Blazm.Components
{
    public interface IGridColumn
    {
        int Priority
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
