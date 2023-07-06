namespace DitheringLib
{
    public interface IErrorDiffusion
    {
        #region Methods

        void Diffuse(ArgbColor[] data, ArgbColor original, ArgbColor transformed, int x, int y, int width, int height);

        bool Prescan { get; }

        #endregion
    }
}
