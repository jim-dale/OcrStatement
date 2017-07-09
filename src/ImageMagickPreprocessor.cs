
namespace OcrStatement
{
    using System;
    using System.IO;
    using ImageMagick;

    public enum ImagePreprocessorFilePolicy
    {
        TempFilePolicy,
        RenamedFilePolicy
    }

    public class ImageMagickPreprocessor : IImagePreprocessor
    {
        public Percentage DeskewPercentage { get; set; }
        public string IntermediateFolder { get; set; }

        private Func<string, string> _getFileName;
        private Action<string> _cleanup;

        public ImageMagickPreprocessor(double percentageDeskew, ImagePreprocessorFilePolicy policy, string intermediateFolder)
        {
            DeskewPercentage = new Percentage(percentageDeskew);
            SetFilePolicy(policy);
            IntermediateFolder = intermediateFolder;

            Directory.CreateDirectory(IntermediateFolder);
        }

        public void SetFilePolicy(ImagePreprocessorFilePolicy policy)
        {
            switch (policy)
            {
                case ImagePreprocessorFilePolicy.TempFilePolicy:
                    SetTempFilePolicy();
                    break;
                case ImagePreprocessorFilePolicy.RenamedFilePolicy:
                    SetRenamedFilePolicy();
                    break;
                default:
                    break;
            }
        }

        public void SetTempFilePolicy()
        {
            _getFileName = GetTempFileName;
            _cleanup = TempFileCleanup;
        }

        public void SetRenamedFilePolicy()
        {
            _getFileName = GetRenamedFileName;
            _cleanup = default(Action<string>);
        }

        public string GetIntermediateFileName(string path)
        {
            string result = _getFileName.Invoke(path);
            return result;
        }

        public void CreateNewImageFrom(string source, string target)
        {
            using (MagickImage image = new MagickImage(source))
            {
                image.Deskew(DeskewPercentage);
                image.Despeckle();
                image.Grayscale(PixelIntensityMethod.Rec709Luminance);

                image.Write(target);
            }
        }

        public void Cleanup(string path)
        {
            _cleanup?.Invoke(path);
        }

        #region Temp File Policy methods
        private string GetTempFileName(string path)
        {
            return Path.GetTempFileName();
        }

        private void TempFileCleanup(string path)
        {
            File.Delete(path);
        }
        #endregion

        #region Renamed File Policy methods
        private string GetRenamedFileName(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);

            string targetFileName = fileName + "-processed" + extension;

            string result = Path.Combine(IntermediateFolder, targetFileName);
            return result;
        }
        #endregion
    }
}
