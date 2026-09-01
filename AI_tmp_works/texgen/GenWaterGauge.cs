using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

// 水位標（量水標）テクスチャ生成（PLATEAU CityHack 2026）
// 日本の量水標に倣い、20cm ごとの紅白帯と 1m ごとの目盛り数字を入れる。
public static class WaterGaugeGen
{
    // 1m あたりのピクセル数。20cm 帯がちょうど 48px になるよう 240 にしている
    private const int PIXELS_PER_METER = 240;
    private const int BAND_PIXELS = 48;      // 20cm 帯
    private const int GAUGE_METERS = 3;      // 標識の全長
    private const int POLE_WIDTH = 96;

    private static readonly Color RED = Color.FromArgb(200, 32, 42);
    private static readonly Color WHITE = Color.FromArgb(245, 245, 240);
    private static readonly Color LINE = Color.FromArgb(40, 40, 40);

    private static void Save(Bitmap bmp, string dir, string name)
    {
        string path = Path.Combine(dir, name);
        bmp.Save(path, ImageFormat.Png);
        bmp.Dispose();
        Console.WriteLine(name + "  " + new FileInfo(path).Length + " bytes");
    }

    private static Font PickFont(float size, FontStyle style)
    {
        // 日本語が出せるフォントを優先して探す
        string[] candidates = { "Yu Gothic UI", "Meiryo", "MS Gothic", "Arial" };
        foreach (string name in candidates)
        {
            FontFamily family = null;
            try { family = new FontFamily(name); }
            catch (ArgumentException) { continue; }
            return new Font(family, size, style, GraphicsUnit.Pixel);
        }
        return new Font(FontFamily.GenericSansSerif, size, style, GraphicsUnit.Pixel);
    }

    public static void Generate(string outDir)
    {
        BuildPole(outDir);
        BuildBoard(outDir);
    }

    /// 支柱に巻く水位標。下端が 0m、上端が GAUGE_METERS。
    /// 縦にタイリングせず、1 枚で全長を覆う想定
    private static void BuildPole(string outDir)
    {
        int height = PIXELS_PER_METER * GAUGE_METERS;
        Bitmap bmp = new Bitmap(POLE_WIDTH, height);

        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // 20cm ごとの紅白帯。画像の上端が高い位置になるので上から数える
            int bandCount = height / BAND_PIXELS;
            for (int i = 0; i < bandCount; i++)
            {
                // 下端を基準に色を決める（0m の帯が赤で始まる）
                int bandFromBottom = bandCount - 1 - i;
                Color color = (bandFromBottom % 2 == 0) ? RED : WHITE;
                g.FillRectangle(new SolidBrush(color), 0, i * BAND_PIXELS, POLE_WIDTH, BAND_PIXELS);
            }

            // 帯の境界に細い線を入れて読み取りやすくする
            using (Pen pen = new Pen(LINE, 2f))
            {
                for (int i = 1; i < bandCount; i++)
                {
                    int y = i * BAND_PIXELS;
                    g.DrawLine(pen, 0, y, POLE_WIDTH * 0.35f, y);
                }
            }

            DrawMeterLabels(g, height);

            // 外枠
            using (Pen pen = new Pen(LINE, 3f))
            {
                g.DrawRectangle(pen, 1, 1, POLE_WIDTH - 3, height - 3);
            }
        }
        Save(bmp, outDir, "WaterGaugePole.png");
    }

    /// 1m ごとに数字を入れる。帯の色に応じて文字色を反転させて読めるようにする
    private static void DrawMeterLabels(Graphics g, int height)
    {
        Font font = PickFont(40f, FontStyle.Bold);
        StringFormat format = new StringFormat();
        format.Alignment = StringAlignment.Far;
        format.LineAlignment = StringAlignment.Center;

        for (int meter = 1; meter <= GAUGE_METERS; meter++)
        {
            int y = height - (meter * PIXELS_PER_METER);

            // その位置の帯が赤かどうかで文字色を決める
            int bandFromBottom = (meter * PIXELS_PER_METER) / BAND_PIXELS;
            bool isOnRed = (bandFromBottom % 2 == 0);
            Brush brush = isOnRed ? Brushes.White : new SolidBrush(LINE);

            // 1m 線
            using (Pen pen = new Pen(isOnRed ? Color.White : LINE, 4f))
            {
                g.DrawLine(pen, 0, y, POLE_WIDTH, y);
            }

            RectangleF rect = new RectangleF(0, y, POLE_WIDTH - 8, BAND_PIXELS);
            g.DrawString(meter.ToString(), font, brush, rect, format);
        }
        font.Dispose();
    }

    /// 想定浸水深の標識板。ポールの上部に付ける看板を想定
    private static void BuildBoard(string outDir)
    {
        const int W = 256;
        const int H = 256;
        Bitmap bmp = new Bitmap(W, H);

        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            Color navy = Color.FromArgb(18, 52, 96);
            g.Clear(navy);

            // 白フチ
            using (Pen pen = new Pen(Color.White, 8f))
            {
                g.DrawRectangle(pen, 6, 6, W - 13, H - 13);
            }

            StringFormat center = new StringFormat();
            center.Alignment = StringAlignment.Center;
            center.LineAlignment = StringAlignment.Center;

            Font title = PickFont(38f, FontStyle.Bold);
            g.DrawString("想定浸水深", title, Brushes.White, new RectangleF(0, 24, W, 48), center);
            title.Dispose();

            // 水面のイメージ（下半分を水色で塗り、波線を引く）
            Color water = Color.FromArgb(70, 150, 200);
            g.FillRectangle(new SolidBrush(water), 14, H / 2, W - 28, H / 2 - 14);

            using (Pen pen = new Pen(Color.White, 3f))
            {
                for (int i = 0; i < 3; i++)
                {
                    float y = H / 2 + 14 + i * 22;
                    g.DrawBezier(pen, 24, y, 80, y - 10, 150, y + 10, W - 24, y);
                }
            }

            // 矢印（水面の高さを指す）
            using (Pen pen = new Pen(Color.White, 6f))
            {
                pen.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(pen, W / 2, H / 2 - 40, W / 2, H / 2 - 6);
            }
        }
        Save(bmp, outDir, "WaterGaugeBoard.png");
    }
}
