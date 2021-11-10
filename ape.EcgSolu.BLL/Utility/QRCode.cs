using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ThoughtWorks.QRCode.Codec;

namespace ape.EcgSolu.BLL.Utility
{
    public class QRCode
    {
        public static Bitmap CreateQRCode(string codeNumber, int size)
        {
            QRCodeEncoder qrCodeEnc = new QRCodeEncoder();
            qrCodeEnc.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEnc.QRCodeVersion = 0;
            qrCodeEnc.QRCodeScale = size;
            qrCodeEnc.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            Bitmap image = qrCodeEnc.Encode(codeNumber, Encoding.UTF8);
            return image;
        }
    }
}
