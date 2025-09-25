using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Nevron.Nov.Diagram.Formats;
using Nevron.Nov.Graphics;
using Nevron.Nov.Text;

namespace Nevron.Nov.Diagram.Converter
{
	/// <summary>
	/// Converts Nevron Diagram drawings and libraries to NOV Diagram drawings and libraries.
	/// </summary>
	public static class NDiagramConverter
	{
		#region Constructors

		/// <summary>
		/// Static constructor - applies Nevron and NOV licenses.
		/// </summary>
		static NDiagramConverter()
		{
			// Nevron License
			Nevron.NLicenseManager.Instance.SetLicense(new Nevron.NLicense(
				"000adf46-0072-02ef-99e7-02e200062199," + // Desktop redistribution key
				"002100d6-d913-423c-196f-5c04f27db2f4"    // Evaluation key (for debugging)
			));
			Nevron.NLicenseManager.Instance.LockLicense = true;

			// NOV License
			NLicenseManager.Instance.SetLicense(new NLicense(
				"59000002577f580202130068056c08550ed9002d96592f57," + // Desktop redistribution key
				"a6160bbd05e03c079104fba32a0093006fb176761dc0ef49"    // Evaluation key (for debugging)
			));
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Creates a form that can be used to visually convert Nevron Diagram drawings to NOV Diagram drawings.
		/// </summary>
		/// <returns></returns>
		public static Form CreateForm()
		{
			return new MainForm();
		}

		/// <summary>
		/// Converts the Nevron Drawing document in the given stream to a NOV Drawing document.
		/// </summary>
		/// <param name="nevronDrawingStream"></param>
		/// <returns></returns>
		public static NDrawingDocument ConvertDrawingFromStream(Stream nevronDrawingStream)
		{
			return ConvertDrawingFromStream(nevronDrawingStream, out _);
		}
		/// <summary>
		/// Converts the Nevron Drawing document in the given stream to a NOV Drawing document.
		/// </summary>
		/// <param name="nevronDrawingStream"></param>
		/// <param name="nevronDrawingDocument"></param>
		/// <returns></returns>
		public static NDrawingDocument ConvertDrawingFromStream(Stream nevronDrawingStream, out Nevron.Diagram.NDrawingDocument nevronDrawingDocument)
		{
			Nevron.Diagram.Extensions.NPersistencyManager persistencyManager = CreatePersistencyManager();
			Nevron.Serialization.PersistencyFormat persistencyFormat = GetPersistencyFormat(nevronDrawingStream);
			persistencyManager.LoadFromStream(nevronDrawingStream, persistencyFormat, null);
			nevronDrawingDocument = (Nevron.Diagram.NDrawingDocument)persistencyManager.PersistentDocument.Sections[0].Object;

			return ConvertDrawing(nevronDrawingDocument);
		}
		/// <summary>
		/// Converts the given Nevron Drawing document to a NOV Drawing document.
		/// </summary>
		/// <param name="nevronDrawingDocument"></param>
		/// <returns></returns>
		public static NDrawingDocument ConvertDrawing(Nevron.Diagram.NDrawingDocument nevronDrawingDocument)
		{
			NDrawingImporter importer = new NDrawingImporter();
			return importer.Import(nevronDrawingDocument);
		}

		/// <summary>
		/// Converst the Nevron Library document in the given stream to a NOV Library document.
		/// </summary>
		/// <param name="nevronLibraryStream"></param>
		/// <returns></returns>
		public static NLibraryDocument ConvertLibraryFromStream(Stream nevronLibraryStream)
		{
			return ConvertLibraryFromStream(nevronLibraryStream, out _);
		}
		/// <summary>
		/// Converst the Nevron Library document in the given stream to a NOV Library document.
		/// </summary>
		/// <param name="nevronLibraryStream"></param>
		/// <param name="nevronLibraryDocument"></param>
		/// <returns></returns>
		public static NLibraryDocument ConvertLibraryFromStream(Stream nevronLibraryStream, out Nevron.Diagram.NLibraryDocument nevronLibraryDocument)
		{
			Nevron.Diagram.Extensions.NPersistencyManager persistencyManager = CreatePersistencyManager();
			Nevron.Serialization.PersistencyFormat persistencyFormat = GetPersistencyFormat(nevronLibraryStream);
			persistencyManager.LoadFromStream(nevronLibraryStream, persistencyFormat, null);
			nevronLibraryDocument = (Nevron.Diagram.NLibraryDocument)persistencyManager.PersistentDocument.Sections[0].Object;

			return ConvertLibrary(nevronLibraryDocument);
		}
		/// <summary>
		/// Converts the given Nevron Library document to a NOV Library document.
		/// </summary>
		/// <param name="nevronLibraryDocument"></param>
		/// <returns></returns>
		public static NLibraryDocument ConvertLibrary(Nevron.Diagram.NLibraryDocument nevronLibraryDocument)
		{
			NLibraryImporter importer = new NLibraryImporter();
			return importer.Import(nevronLibraryDocument);
		}

		/// <summary>
		/// Checks whether the given stream contains a NOV drawing.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static bool IsNovDrawing(Stream stream)
		{
			byte[] header = ReadHeader(stream);
			if (header == null)
				return false;

			int matchScore;
			NDrawingFormatRegistry.Instance.GetFromExtensionAndHeader(null, null, true, out matchScore);
			return matchScore > 0;
		}
		/// <summary>
		/// Checks whether the given stream contains a NOV library.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static bool IsNovLibrary(Stream stream)
		{
			byte[] header = ReadHeader(stream);
			if (header == null)
				return false;

			int matchScore;
			NLibraryFormatRegistry.Instance.GetFromExtensionAndHeader(null, null, true, out matchScore);
			return matchScore > 0;
		}

		#endregion

		#region Internal Methods - Coordinates

		/// <summary>
		/// Tries to convert the given Nevron measurement unit to a NOV unit.
		/// </summary>
		/// <param name="nevronUnit"></param>
		/// <param name="novUnit"></param>
		/// <returns></returns>
		internal static bool TryConvertUnit(GraphicsCore.NMeasurementUnit nevronUnit, out NUnit novUnit)
		{
			// Try to get a unit via reflection
			FieldInfo field = typeof(NUnit).GetField(nevronUnit.Name);
			if (field != null)
			{
				novUnit = (NUnit)field.GetValue(null);
				return true;
			}

			novUnit = NUnit.DIP;
			return false;
		}
		internal static NMatrix ToNMatrix(GraphicsCore.NMatrix2DF nevronMatrix)
		{
			return new NMatrix(
					nevronMatrix.M11,
					nevronMatrix.M12,
					nevronMatrix.M21,
					nevronMatrix.M22,
					nevronMatrix.DX,
					nevronMatrix.DY);
		}

		internal static NPoint ToNPoint(NPage novPage, GraphicsCore.NPointF nevronPoint)
		{
			return new NPoint(
				ConvertCoordinate(novPage, nevronPoint.X),
				ConvertCoordinate(novPage, nevronPoint.Y)
			);
		}
		internal static NPoint[] ToNPoints(NPage novPage, GraphicsCore.NPointF[] nevronPoints)
		{
			NPoint[] arr = new NPoint[nevronPoints.Length];
			for (int i = 0; i < nevronPoints.Length; i++)
			{
				arr[i] = ToNPoint(novPage, nevronPoints[i]);
			}

			return arr;
		}

		internal static NPoint ToNPoint(NPage novPage, System.Drawing.PointF nevronPoint)
		{
			return new NPoint(
				ConvertCoordinate(novPage, nevronPoint.X),
				ConvertCoordinate(novPage, nevronPoint.Y)
			);
		}
		internal static NPoint[] ToNPoints(NPage novPage, System.Drawing.PointF[] nevronPoints)
		{
			NPoint[] points = new NPoint[nevronPoints.Length];
			for (int i = 0; i < nevronPoints.Length; i++)
			{
				points[i] = ToNPoint(novPage, nevronPoints[i]);
			}

			return points;
		}

		internal static NRectangle ToNRectangle(NPage novPage, GraphicsCore.NRectangleF nevronRectangle)
		{
			return new NRectangle(
				ConvertCoordinate(novPage, nevronRectangle.X),
				ConvertCoordinate(novPage, nevronRectangle.Y),
				ConvertCoordinate(novPage, nevronRectangle.Width),
				ConvertCoordinate(novPage, nevronRectangle.Height)
			);
		}

		internal static double ConvertCoordinate(NPage page, double coordinate)
		{
			return page != null ? page.LogicalToDips(coordinate) : coordinate;
		}

		#endregion

		#region Internal Methods - Images

		/// <summary>
		/// Converts the given <see cref="System.Drawing.Bitmap"/> to a NOV image.
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		internal static NImage ToNImage(System.Drawing.Bitmap bitmap)
		{
			byte[] imageData;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				System.Drawing.Imaging.ImageFormat imageFormat = bitmap.RawFormat;
				if (imageFormat.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp))
				{
					// This is a memory bitmap, so change the image format to PNG
					imageFormat = System.Drawing.Imaging.ImageFormat.Png;
				}

				bitmap.Save(memoryStream, imageFormat);
				imageData = memoryStream.ToArray();
			}

			return NImage.FromBytes(imageData);
		}

		#endregion

		#region Internal Methods - Serialization

		internal static Nevron.Diagram.Extensions.NPersistencyManager CreatePersistencyManager()
		{
			Nevron.Diagram.Extensions.NPersistencyManager persistencyManager = new Nevron.Diagram.Extensions.NPersistencyManager();
			persistencyManager.Serializer.XmlExtraTypes = new Type[] {
				typeof(IDSDataObjectAdaptor)
			};

			return persistencyManager;
		}

		#endregion

		#region Implementation

		/// <summary>
		/// Reads the header of the given stream and restores the reading position back to its value prior to calling this method.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		private static byte[] ReadHeader(Stream stream)
		{
			if (!stream.CanSeek)
				return null;

			int headerLength = (int)NMath.Min(HeaderLength, stream.Length - stream.Position);
			byte[] header = new byte[headerLength];

			long oldPosition = stream.Position;
			stream.Read(header, 0, headerLength);
			stream.Position = oldPosition;

			return header;
		}

		private static Nevron.Serialization.PersistencyFormat GetPersistencyFormat(Stream stream)
		{
			return IsXml(stream) ? Nevron.Serialization.PersistencyFormat.XML : Nevron.Serialization.PersistencyFormat.Binary;
		}
		/// <summary>
		/// Checks whether the given stream contains a XML document.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		private static bool IsXml(Stream stream)
		{
			byte[] header = ReadHeader(stream);
			if (header == null || header.Length < XmlFileSignature.Length)
				return false;

			for (int i = 0; i < XmlFileSignature.Length; i++)
			{
				if (header[i] != XmlFileSignature[i])
					return false;
			}

			return true;
		}

		#endregion

		#region Constants

		/// <summary>
		/// The number of bytes to read for header.
		/// </summary>
		private const int HeaderLength = 64;
		private static readonly byte[] XmlFileSignature = NEncoding.UTF8.GetBytes("<?xml ");

		#endregion
	}
}