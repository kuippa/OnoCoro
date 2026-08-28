using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

// 廃材用テクスチャ生成（CityHack 2026）
// 64x64 のシームレス（上下左右がつながる）値ノイズを作り、素材ごとに着色して PNG 出力する。
public static class DebrisTexGen
{
    private const int SIZE = 64;

    // 格子を巡回参照させることでタイリング可能にする
    private static double[,] NoiseLayer(int lattice, int seed)
    {
        Random rnd = new Random(seed);
        double[,] lat = new double[lattice, lattice];
        for (int y = 0; y < lattice; y++)
        {
            for (int x = 0; x < lattice; x++) { lat[x, y] = rnd.NextDouble(); }
        }

        double[,] outMap = new double[SIZE, SIZE];
        double scale = (double)lattice / SIZE;
        for (int y = 0; y < SIZE; y++)
        {
            double fy = y * scale;
            int iy = (int)Math.Floor(fy);
            double ty = Smooth(fy - iy);
            int iy0 = iy % lattice;
            int iy1 = (iy0 + 1) % lattice;
            for (int x = 0; x < SIZE; x++)
            {
                double fx = x * scale;
                int ix = (int)Math.Floor(fx);
                double tx = Smooth(fx - ix);
                int ix0 = ix % lattice;
                int ix1 = (ix0 + 1) % lattice;
                double top = lat[ix0, iy0] * (1 - tx) + lat[ix1, iy0] * tx;
                double btm = lat[ix0, iy1] * (1 - tx) + lat[ix1, iy1] * tx;
                outMap[x, y] = top * (1 - ty) + btm * ty;
            }
        }
        return outMap;
    }

    private static double Smooth(double t) { return t * t * (3.0 - 2.0 * t); }

    // 複数のオクターブを重ねて、粗さと細かさを両立させる
    private static double[,] Fbm(int seed)
    {
        double[,] o1 = NoiseLayer(4, seed);
        double[,] o2 = NoiseLayer(8, seed + 101);
        double[,] o3 = NoiseLayer(16, seed + 202);
        double[,] outMap = new double[SIZE, SIZE];
        for (int y = 0; y < SIZE; y++)
        {
            for (int x = 0; x < SIZE; x++)
            {
                outMap[x, y] = o1[x, y] * 0.55 + o2[x, y] * 0.30 + o3[x, y] * 0.15;
            }
        }
        return outMap;
    }

    private static int Clamp255(double v)
    {
        if (v < 0) { return 0; }
        if (v > 255) { return 255; }
        return (int)v;
    }

    private static void Save(Bitmap bmp, string dir, string name)
    {
        string path = Path.Combine(dir, name);
        bmp.Save(path, ImageFormat.Png);
        bmp.Dispose();
        Console.WriteLine(name + "  " + new FileInfo(path).Length + " bytes");
    }

    public static void Generate(string outDir)
    {
        BuildConcrete(outDir);
        BuildWood(outDir);
        BuildMetal(outDir);
        BuildMixed(outDir);
    }

    // コンクリート殻（RC/SRC 造の廃材）
    private static void BuildConcrete(string outDir)
    {
        double[,] n = Fbm(1234);
        Random grain = new Random(11);
        Bitmap bmp = new Bitmap(SIZE, SIZE);
        for (int y = 0; y < SIZE; y++)
        {
            for (int x = 0; x < SIZE; x++)
            {
                double v = 96 + (n[x, y] - 0.5) * 110 + (grain.NextDouble() - 0.5) * 26;
                bmp.SetPixel(x, y, Color.FromArgb(255, Clamp255(v), Clamp255(v), Clamp255(v * 1.03)));
            }
        }
        Save(bmp, outDir, "DebrisConcrete.png");
    }

    // 木材（木造の廃材。横方向の板目と 16px ごとの継ぎ目）
    private static void BuildWood(string outDir)
    {
        double[,] n = Fbm(5678);
        Random grain = new Random(22);
        Bitmap bmp = new Bitmap(SIZE, SIZE);
        for (int y = 0; y < SIZE; y++)
        {
            bool isSeam = (y % 16) == 0;
            for (int x = 0; x < SIZE; x++)
            {
                // 木目: x 方向に伸びる縞にするため y 座標へノイズを効かせる
                double streak = Math.Sin((y + n[x, y] * 9.0) * 1.9) * 0.5 + 0.5;
                double t = streak * 0.6 + n[x, y] * 0.4;
                double r = 118 + t * 78;
                double g = 80 + t * 56;
                double b = 46 + t * 32;
                if (isSeam) { r *= 0.55; g *= 0.55; b *= 0.55; }
                double j = (grain.NextDouble() - 0.5) * 20;
                bmp.SetPixel(x, y, Color.FromArgb(255, Clamp255(r + j), Clamp255(g + j), Clamp255(b + j)));
            }
        }
        Save(bmp, outDir, "DebrisWood.png");
    }

    // 金属（鉄骨造の廃材。錆の斑を乗せる）
    private static void BuildMetal(string outDir)
    {
        double[,] n = Fbm(9012);
        double[,] rust = Fbm(3141);
        Random grain = new Random(33);
        Bitmap bmp = new Bitmap(SIZE, SIZE);
        for (int y = 0; y < SIZE; y++)
        {
            for (int x = 0; x < SIZE; x++)
            {
                double baseV = 118 + (n[x, y] - 0.5) * 70;
                double r = baseV;
                double g = baseV;
                double b = baseV * 1.05;
                // 錆はしきい値を超えた部分だけに出して斑にする
                double rv = rust[x, y];
                if (rv > 0.55)
                {
                    double k = (rv - 0.55) / 0.45;
                    r = r * (1 - k) + 146 * k;
                    g = g * (1 - k) + 76 * k;
                    b = b * (1 - k) + 38 * k;
                }
                double j = (grain.NextDouble() - 0.5) * 22;
                bmp.SetPixel(x, y, Color.FromArgb(255, Clamp255(r + j), Clamp255(g + j), Clamp255(b + j)));
            }
        }
        Save(bmp, outDir, "DebrisMetal.png");
    }

    // 混合廃材（種別を問わない汎用。木・コンクリ・錆をノイズで混ぜる）
    private static void BuildMixed(string outDir)
    {
        double[,] n = Fbm(2468);
        double[,] sel = NoiseLayer(8, 777);
        Random grain = new Random(44);
        Bitmap bmp = new Bitmap(SIZE, SIZE);
        for (int y = 0; y < SIZE; y++)
        {
            for (int x = 0; x < SIZE; x++)
            {
                double s = sel[x, y];
                double t = n[x, y];
                double r, g, b;
                if (s < 0.40) { r = 104 + t * 60; g = 100 + t * 58; b = 98 + t * 56; }
                else if (s < 0.72) { r = 122 + t * 70; g = 84 + t * 50; b = 50 + t * 30; }
                else { r = 132 + t * 40; g = 74 + t * 40; b = 44 + t * 26; }
                double j = (grain.NextDouble() - 0.5) * 24;
                bmp.SetPixel(x, y, Color.FromArgb(255, Clamp255(r + j), Clamp255(g + j), Clamp255(b + j)));
            }
        }
        Save(bmp, outDir, "DebrisMixed.png");
    }
}
