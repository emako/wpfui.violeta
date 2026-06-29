using Wpf.Ui.Violeta.Controls.Encoding.DataEncodation;
using Wpf.Ui.Violeta.Controls.Encoding.EncodingRegion;
using Wpf.Ui.Violeta.Controls.Encoding.ErrorCorrection;
using Wpf.Ui.Violeta.Controls.Encoding.Masking;
using Wpf.Ui.Violeta.Controls.Encoding.Masking.Scoring;
using Wpf.Ui.Violeta.Controls.Encoding.Positioning;

namespace Wpf.Ui.Violeta.Controls.Encoding;

internal static class QRCodeEncode
{
	internal static BitMatrix Encode(string content, ErrorCorrectionLevel errorLevel)
	{
		EncodationStruct encodeStruct = DataEncode.Encode(content, errorLevel);

		return ProcessEncodationResult(encodeStruct, errorLevel);
	}

	private static BitMatrix ProcessEncodationResult(EncodationStruct encodeStruct, ErrorCorrectionLevel errorLevel)
	{
		BitList codewords = ECGenerator.FillECCodewords(encodeStruct.DataCodewords, encodeStruct.VersionDetail);

		TriStateMatrix triMatrix = new(encodeStruct.VersionDetail.MatrixWidth);
		PositioningPatternBuilder.EmbedBasicPatterns(encodeStruct.VersionDetail.Version, triMatrix);

		triMatrix.EmbedVersionInformation(encodeStruct.VersionDetail.Version);
		triMatrix.EmbedFormatInformation(errorLevel, new Pattern0());
		triMatrix.TryEmbedCodewords(codewords);

		return triMatrix.GetLowestPenaltyMatrix(errorLevel);
	}
}


