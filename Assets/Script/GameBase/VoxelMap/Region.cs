using System.Collections.Generic;

public class Region
{
    public Dictionary<(int, int, int), Chunk> loadedChunks = new Dictionary<(int, int, int), Chunk>();
    public int regionX, regionY, regionZ;

    public Region(int regionX, int regionY, int regionZ)
    {
        this.regionX = regionX;
        this.regionY = regionY;
        this.regionZ = regionZ;
    }

    public Chunk GetChunk(int chunkX, int chunkY ,int chunkZ)
    {
        if (!loadedChunks.ContainsKey((chunkX, chunkY, chunkZ))) 
        {
            loadedChunks[(chunkX, chunkY, chunkZ)] = new Chunk(chunkX, chunkY, chunkZ);
        }
        return loadedChunks[(chunkX, chunkY, chunkZ)];
    }
}