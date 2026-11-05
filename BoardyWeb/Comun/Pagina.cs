namespace BoardyWeb.Comun
{
    public class Pagina<T>
    {
        public const int Tamanio = 50;

        public IReadOnlyList<T> Elementos { get; init; } = new List<T>();

        public int Numero { get; init; }

        public int TotalElementos { get; init; }

        public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(TotalElementos / (double)Tamanio));
    }
}
