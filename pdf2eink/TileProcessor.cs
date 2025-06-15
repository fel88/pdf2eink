namespace pdf2eink
{
    public class TileProcessor
    {
        public int[,] map;
        List<HashSet<int>> groups = new List<HashSet<int>>();
        Bitmap bmp;
        public void Init(Bitmap bmp)
        {
            this.bmp = bmp;

            int currentMark = 1;

            for (int i = 0; i < bmp.Width; i++)
            {
                bool active = false;
                for (int j = 0; j < bmp.Height; j++)
                {
                    var px = bmp.GetPixel(i, j);
                    if (px.R == 0)
                    {
                        if (!active)
                        {
                            currentMark++;
                            active = true;
                        }

                    }
                    if (px.R > 0)
                    {
                        if (active)
                        {

                            active = false;
                        }

                    }
                    if (active)
                        map[i, j] = currentMark;
                }
                currentMark++;
            }
        }

        public void SimplifyMarks()
        {

            for (int i = 0; i < bmp.Width; i++)
            {

                for (int j = 0; j < bmp.Height; j++)
                {
                    if (map[i, j] != 0)
                    {
                        var gr1 = groups.FirstOrDefault(z => z.Contains(map[i, j]));
                        if (gr1 != null)
                            map[i, j] = gr1.Min();
                    }
                }
            }
        }

        public TileInfo[] ExtractTiles()
        {
            List<TilePoint> points = new List<TilePoint>();
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    if (map[i, j] == 0)
                        continue;
                    points.Add(new TilePoint()
                    {
                        X = i,
                        Y = j,
                        Group = map[i, j]
                    });
                }
            }
            Dictionary<int, TilePoint[]> pre = points.GroupBy(z => z.Group).ToDictionary(z => z.Key, y => y.ToArray());
            List<TileInfo> ret = new List<TileInfo>();
            List<Tile> tiles = new List<Tile>();
            foreach (var item in pre)
            {
                TileInfo ti = new TileInfo();
               
                var maxx = item.Value.Max(z => z.X);
                var maxy = item.Value.Max(z => z.Y);
                var minx = item.Value.Min(z => z.X);
                var miny = item.Value.Min(z => z.Y);
                ti.X = minx;
                ti.Y = miny;
                Tile t = new Tile(item.Value.Select(z => new TilePoint() { X = z.X - minx, Y = z.Y - miny, Group = z.Group }).ToArray());
                
              
                ti.Tile = t;
                tiles.Add(t);
                ret.Add(ti);
            }
            tiles = DistinctTiles(tiles.ToArray()).ToList();

            foreach (var item in ret)
            {
                item.Tile = tiles.First(z => z.ImageHash == item.Tile.ImageHash);
            }
            return ret.ToArray();
        }

        public Tile[] DistinctTiles(Tile[] tiles)
        {                       
           return tiles.GroupBy(z => z.ImageHash).Select(z => z.First()).ToArray();            
        }

        public void MakeGroups()
        {
            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 1; i < bmp.Width; i++)
                {

                    var m1 = map[i - 1, j];
                    var m2 = map[i, j];
                    if (m1 != m2 && m1 != 0 && m2 != 0)
                    {
                        if (!groups.Any(z => z.Contains(m1) && z.Contains(m2)))
                        {
                            var gr1 = groups.FirstOrDefault(z => z.Contains(m1) || z.Contains(m2));
                            if (gr1 == null)
                            {
                                groups.Add(new HashSet<int>());
                                groups.Last().Add(m1);
                                groups.Last().Add(m2);
                            }
                            else
                            {
                                gr1.Add(m1);
                                gr1.Add(m2);
                            }
                        }
                    }
                }
            }
        }
    }
}