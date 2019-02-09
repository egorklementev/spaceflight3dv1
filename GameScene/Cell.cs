public class Cell {

    public bool IsEmpty { get; set; }
    public Gem GemInside { get; set; }

    public Cell ()
    {
        IsEmpty = true;
        GemInside = null;
    }
    
}
