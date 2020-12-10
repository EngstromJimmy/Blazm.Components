using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoFixture;
using Bunit;
using FluentAssertions;
using Xunit;

namespace Blazm.Components
{
    public class BlazmGridTest : TestContext
    {
        private AutoFixture.Fixture fixture = new AutoFixture.Fixture();

        public BlazmGridTest()
        {
            Services.AddBlazm();
        }

        [Fact]
        public void Will_Render_Empty_Table()
        {
            var cut = RenderComponent<BlazmGrid<string>>();

            var table = cut.Find("table");

            table.MarkupMatches(@"<table class=""table table-striped"" id:ignore=""table[.*]"">
                                   <thead>
                                      <tr></tr>
                                    </thead>
                                    <tbody></tbody>
                                    <tbody>
                                      <tr>
                                        <td class=""alignCenter"" colspan=""0""></td>
                                      </tr>
                                    </tbody>
                                  </table>");
        }

        [Fact]
        public void Renders_Column_Name_Based_On_Display_Attribute()
        {
            var testData = fixture.Create<List<Data>>();
            var cut = RenderComponent<BlazmGrid<Data>>(parameters => parameters
                .Add(p => p.Data, testData)
                .Add(p => p.AutoGenerateColumns, true)
            );

            cut.WaitForAssertion(() =>
            {
                cut.Find("thead tr th")
                    .TextContent
                    .Should()
                    .Be("Comes from attribute");
            });
        }

        [Fact]
        public void Renders_Column_Name_Based_On_Name()
        {
            var testData = fixture.Create<List<Data>>();
            var cut = RenderComponent<BlazmGrid<Data>>(parameters => parameters
                .Add(p => p.Data, testData)
                .Add<GridColumn>(p => p.GridColumns, gcs => gcs
                    .Add(g => g.Field, nameof(Data.String))
                    .Add(g => g.Title, "FOO")
                )
                .Add<GridColumn>(p => p.GridColumns, gcs => gcs
                    .Add(g => g.Field, nameof(Data.String))
                    .Add(g => g.Title, "FOO")
                )
            );

            cut.Find("thead tr th")
                .TextContent
                .Should()
                .Be("FOO");
        }

        class Data
        {
            [Display(Name = "Comes from attribute")]
            public string String { get; set; }
        }
    }
}
