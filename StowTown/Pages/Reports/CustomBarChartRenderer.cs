using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.Pages.Reports
{
    public class CustomBarChartRenderer : BarChart
    {
        protected override void DrawBarArea(SKCanvas canvas, float headerHeight, SKSize itemSize, SKSize barSize, SKColor color, SKColor otherColor, float origin, float value, float barX, float barY)
        {
            // Calculate the filled portion of the bar
            float filledHeight = barSize.Height * value;

            // Draw the filled portion
            using (var paint = new SKPaint { Color = color })
            {
                canvas.DrawRect(barX, barY - filledHeight, barSize.Width, filledHeight, paint);
            }

            // Draw the unfilled portion (optional)
            using (var paint = new SKPaint { Color = SKColors.LightSkyBlue }) // Light color
            {
                canvas.DrawRect(barX, barY - barSize.Height, barSize.Width, barSize.Height - filledHeight, paint);
            }
        }
    }
}
