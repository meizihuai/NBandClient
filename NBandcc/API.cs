using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NBandcc
{
    class API
    {
        public static async Task<NormalResponse> UploadFileBlock(SendQueue send)
        {
            return await HttpHelper.Post("/api/Default/UploadFileBlock", send);
        }
    }
}
