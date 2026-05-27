namespace Wpf.Ui.Violeta.Controls.Encoding.DataEncodation.InputRecognition;

internal struct RecognitionStruct
{
    public RecognitionStruct(string encodingName)
        : this()
    {
        EncodingName = encodingName;
    }

    public string EncodingName { get; private set; }
}
