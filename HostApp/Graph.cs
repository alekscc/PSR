namespace HostApp
{
    public class Graph
    {

        public int[][] matrix;
    
        public Graph(string path)
        {
            ReadFile readFile = new ReadFile();
            matrix = readFile.ReadData(path);
        }
    }
}