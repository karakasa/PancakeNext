using Eto.Drawing;
using Eto.Forms;
using Grasshopper2;
using Grasshopper2.Data;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Primitives;
using Grasshopper2.UI.Skinning;
using GrasshopperIO;
using PancakeNextCore.Attributes;
using PancakeNextCore.Components;
using PancakeNextCore.Interfaces;
using PancakeNextCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeNextCore.GH.Params;

[IoId("3423D60E-E961-4727-95B1-A46C549CE0B5")]
[ComponentCategory("misc")]
public sealed class BigRedButton : PancakeParameter<bool, BigRedButton>, IPancakeLocalizable<BigRedButton>, ISolutionPriority
{
    public static string StaticLocalizedName => Strings.SetupTooltip_TrueOnlyButton;
    public static string StaticLocalizedDescription => Strings.TrueOnlyBtn_WhenClickedTheButtonObjectOnlyRaisesRecomputationOneTime;
    public BigRedButton() { }

    public BigRedButton(IReader reader) : base(reader) { }
    private sealed class BigRedButtonAttributes(BigRedButton button) : ParameterAttributes<BigRedButton>(button), IResponsiveAttributes
    {
        public override bool HasInlet => false;
        public override bool HasOutlet => true;

        public ResponsiveAttributesState ResponsiveState { get; set; }

        public override void Invalidate()
        {
        }
        const float BoundedRadius = 48;
        public override void Layout(Shape shape)
        {
            Bounds = RectangleF.FromCenter(Pivot, new SizeF(BoundedRadius * 2, BoundedRadius * 2));
        }
        protected override void Draw(Context context, Skin skin, Capsule capsule)
        {
            const float Radius = BoundedRadius - 4;

            capsule.AddOutputPlug(Outlet.Y, GripKind.Normal);
            context.Graphics.DrawShape(capsule.Slab, skin.Shades[Owner]);

            var center = capsule.Slab.Apex.Center;
            var g = context.Graphics;
            var c = new CircleF(center, Radius);

            using var brush = new LinearGradientBrush(capsule.Slab.Apex, Color.FromArgb(0x00F23D3D), Colors.White, 45);

            g.FillCircle(Color.FromRgb(unchecked((int)0x20FFFFFF)), new(center.X + 4, center.Y + 4, Radius));
            g.FillCircle(brush, c);
            g.DrawCircle(Color.FromRgb(0x00F29C94), 5, c);
            capsule.DrawGrips(context.Graphics, skin);

            g.DrawCenteredText(center, StandardFonts.Mono(FontSize.Normal, FontStyle.Bold), skin.Shades[Owner].Text, "FIRE!");
        }

        public Response RespondToMouseDown(MouseEventArgs args) => Response.Ignored;

        public Response RespondToMouseMove(MouseEventArgs args) => Response.Ignored;

        public Response RespondToMouseUp(MouseEventArgs args) => Response.Ignored;

        public Response RespondToMouseDoubleClick(MouseEventArgs args) => Response.Ignored;

        public Response RespondToKeyDown(KeyEventArgs args) => Response.Ignored;

        public Response RespondToKeyUp(KeyEventArgs args) => Response.Ignored;
        public Response RespondToMouseSingleClick(MouseEventArgs args)
        {
            if ((args.Buttons & MouseButtons.Primary) != 0)
            {
                Owner.TriggerNewSolution();
                return Response.Handled;
            }

            return Response.Ignored;
        }
    }

    protected override IAttributes CreateAttributes() => new BigRedButtonAttributes(this);
    private void TriggerNewSolution()
    {
        Expire();
        Document?.Solution.Start();
    }
    private static Tree<bool> GetTreeWithSingleTrue()
    {
        return Garden.TreeEmpty<bool>().Add(Grasshopper2.Data.Path.Zero, true);
    }
    protected override SolutionData ProcessResult(Solution solution, SolutionData intermediateResult)
    {
        intermediateResult.SetTree(GetTreeWithSingleTrue());
        return intermediateResult;
    }
    protected override IIcon? IconInternal => IconHost.CreateFromPathResource("TrueButton");
}
