using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MusicVideoPlayer.Misc
{
    class Base64Sprites
    {
        public static Sprite DownloadIcon;
        public static Sprite PlayIcon;
        public static Sprite ThinRingIcon;

        //https://www.flaticon.com/free-icon/download_724933
        public static string DownloadIconB64 = @"iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAACXBIWXMAAA3VAAAN1QE91ljxAAAAB3RJTUUH4gscDB0RZoF8lQAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMS40E0BoxAAAArdJREFUeF7tmr1rFFEUxXdNRI0gklgIkoiNjYgERBAVKytB0D/ARixsUgiWlmJhKWIhgo2NncRSu4BJZ2GhxAgKCSIELPzAr11/d+cUksnsuDM7O/Oe9weH2eTdd989Jx+72UnLGRHdbrfd6XQmuc5kifVprru1JS4wdxy9Rx1MbgpLv9EyOqht8YC/R4nNfAjgtrbFA6aeyl8u1D7UtnjwADwAD8ADkL9cPAAPwAPwALQtHjwAD8AD8ADkLxcPwAPwADwAbYsHD8AD8AA8APnLJbgAGPg0WkDv+uib/OVC7ecNezfqGTqq4+uFeScYZiUZfXRw5isubY1RHwyyE31IxhodnLnKpf4AbAiGuYYyb3oOGzsLLmuE+mGmrQx0z6ZKRqwOjrA7yDd5WP9X/28YyH4XzFcZgvWG+zwc17HNguEm0VIy7vCh9zyXCR3XTBhyGi0nIw8PBbtHxzQbhj2EPiajl4deb9EBtQ8DBj6BvshDGT7RpxkvegaFwS+gXzIyMLYXnVe7MMHAVfkZGPbOqU244GMMI3fRPz89Wi3cUovwwcwO9NhcyWMmqnmAxrQ9DvA1hZ73XPaBmidc6nmu5/BZ9KKPSv0DI8bsv0EzXyOwtoimVF4I9p9Cm83eE8fsVWkaFk/2JsmABkdUWhh6HEap1wh87g3ar7LC0OOsWmYxo9I0LFYegEEfewPlq9pa33U0q+VS0Kf5ARj0OofWkL3Dc0afLg29wgjAoGXbpA+HQlABVIEH4AF4AB6Axs3CA1BpGhbzAriBrjRcdzRuFsUDiAQPQHbTsPh/B8DPzzFU+D28poO372if7KaxRbSu+ujA2wraJbtpqLEbnddR5ff4Rg2WfqJLPOz/xxcF4xTOodfoR293wODBvu1foot8uEU286HYgtjGdXvIkodm3kytn1brD4RzPZwIzvgdAAAAAElFTkSuQmCC";

        //https://www.flaticon.com/free-icon/media-play-symbol_31128
        public static string PlayIconB64 = @"iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAACXBIWXMAAA3VAAAN1QE91ljxAAAAB3RJTUUH4gscDCQK4mA4wwAAABh0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMS40E0BoxAAABG5JREFUeF7tm0uITmEcxuczboMxkuQ2ioXrwoIkkmtSlI2iRC412bBgNwtZSLFkg7JTLtEsbBRRWGiSGrnEYkgkgxmNGabvMuP3mP9hGszlm/ec875fnnp63/d857z/5/+c97zn+pX9xyDQ1dU1Ac6G8+BMONp+Kn10dnZOhcfhs0Kh8AXew4Aa2jNsldIGiR6B7ST9C5iQZdl5uIzmCFu19EBy5SRZDws/M+8BTCiw/H4+n99JOdE2KS2QWAV8ZTn/ARkDGuEx6nNZlLFNSwMkNKYvAyKwzmdMuJLL5dZRH2Wbhw9yG5ABAut9x4SHVDVBlsYhQTIDNkBgXc0Lb+EpOItFYR8SJDAoAyKwTSuj4Q7lKprhniUQX5QBAtvlMOEN5UFYYV2GBfIo2gCBbYWv8CKcxqKwDgkED8mACPSh0fCScg3NcExArBMDBPrRRYPOFEdpDrcQfgOhzgyI0O1D4S5ltYXxF+h1bkAETHhH3xsslJ9AZ2wGCPTdwr1ELdUq6N/cgKhYDRAYCd+IcQkuoenXcwYExW6AQAxdQequcxeczKJhJiFdICQRAyIQS5fRJ+EimiNNRnpARKIGCMTTIXGdO8utlJUmJR2gJ3EDBGIKT+2aQc8g0zkkCJyKARGI3QyvUl0Ny01WciBoqgYIxM/BBk6XhynHmrRkQPzUDYiAjg8cEpcp55i8+EFcbwwQ0NIBn8AdNDMmMz4QxCsDBPQInxgNpynHm9R4QDzvDIiArm+YcINyocl1D+J4a4CANj1naKDcQtP9qZJOvTZAQJ/wGu6j6fY5Ax16b0AEdD6iWG/S3YAOQzKgjWuF/SbdDeg3JANaMaDGpLsB/QZhABqzTIa3s9nscpPuBvQdwiTYDm9x97iJptv7BTr01gB0CU3wHNTTpIzJdgc69dYAhvwztB2CU02uexDHOwPQo7vDOob8Zsp4X8UTzysD2Ovv0VML55vEeEFMbwzQLK+9TrXK5MUPgqVuAPHbSP4E5QKayb5SI2CqBpD4Cy5utlOdBDMmKzkQNBUDSFwvEK9RXQzTe1lC8MQNIF4L1LeJ1TTTfUGCgEQNIJbO7duojoPJD/neQEQiBhAjD/V0ZynN5B9//wuIid0AjvU2YpyB02mmv9d7AkGxGkDfzczyet7v58eVaIzFAPrMs+f1zdBaC+Un0OrUAPoSm0j+As0pFsZfINKZAfTTQeL1DPndNP2Z6PoCQp0YQOJ6738WLqSZse79B2KHZADb6vR2l2oNTO4mxhUQXbQBbKdP6LXXV9AM83thhBdlAIk/5ljXN8LpX84OBYgflAGsq4mujnIjrGRROMf730ACAzaAxHV60wdOum9P/wMnFyCRfg3gd926PmfI692c7tvDHfK9QTKjya9Rif4N/Ka3s/pjxEoY5n8C+gI5ZkjsphLtTvk3tAzqv4Oa6ML4+rsYkOAe2Nqddjdof4cHYAXNsCe6/kCC+vPkXviA4f4RapbXm5gwLmddQHtZCfdgae/10kdZ2Q+z12yjLMtPoQAAAABJRU5ErkJggg==";
        
        //https://www.flaticon.com/free-icon/oval_136832
        public static string ThinRing64 = @"iVBORw0KGgoAAAANSUhEUgAAADwAAAA8CAQAAACQ9RH5AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAAmJLR0QAAKqNIzIAAAAJcEhZcwAADdcAAA3XAUIom3gAAAAHdElNRQfiDBMVDAsxjvJGAAAD3klEQVRYw8WZTWxUVRSAv/dKaWhpKZH+sIBY0RaWWtAobcNCQzQxgaSiCa5dKIImGhcEqGtMlAVbEkhYagTSVjQVApWEAroTBhIWYCiWMkV+pjMF5nPBNJb+DG86M8+ze+eec7533n3v3PvODShArKGDV2hjNQ0spQZ4wBgjJEhwgcEgVUi0aMgmP/e0E+aTjKfcYWO0iEEEaBdf8DYLgMdc4DSXSPAXd7hPQA31rKCVNXTQTgXwkH72BoPFZrrBU6pOeMRul+S1XeJ7Hsk9lZN2zR+63MOqjrrbhsheDe7xtpr1kE3zwXY7pj5wp4sL9q11lyk16ebCHKvcr+oxn5/382qxV826z4VRXRZ7XE27wwivXt5IH5lWB6yLYtzgefWG7cVBc9HWOawOuexZhrWeV6/6YimwALaYUM9Zm8+oygH1is2lwgLY7BX1eJ65dr/6ty+VEpvL+ob67VzD3Wq6NHM7I/Y602bdNNvQcsfUbeXAArhdTc5SyT2sHisXFgzsVQ9OV29QH8y/XERCt5gya+fTylPqznJiAdytnpiq6FJv5/3SSgOuNal2/Kc4qu4uNxbAHvXHyYsmHzoRfeErCtzgxBNWCGxlAX3BrTjAwS1+opIPntzFabU7DiyAW3IvmNVmfGR9bOB6H5mxGjeqZ+PCAnhOfTOkHSh2T1iYDAJrQ9qAP2MFXwRaQ1qBy7GCE0BbSDNwPVbwNWB5SB1wN1bwPaAWMxp5A1oSsUpNh7HmOkVC7gMF/ysUJbXAvZC7QJQNd8nBN4EVsYJXAsMhl4G2WMGrgUTIJWDN/wH+HegoNlZB0gmcx2rTsS+LaReFQYohKngrtnw3UsGZYDwEfgC2xgb+EPj+SfKNkxuw8ouNTphxGYQQjNBPJR/Hku8nVNIXjE7ex+sxbejrTKrrp6pOqrvKDu5RB55WdZk1ZUtZsascN+sb09WH1N5i+zx5sIH96oGZA00m1e1lA3+mjs767bjZrBnXlQX7qhmzvjvX8D51xNaSY19wWN07t8FCfy5bu6nPynxG1Z5Rr5au5ZRrsA09sxXrMofU4dLMta95Uz3rc1GMa+xXH/pVCZqoGfWXyDXRhX5nVu2df0lxlf1q1m/yzu0sjptMqin3FF7DrbPHlDo65weU173Jg2bVpD0FHA00+rVJNeuBIpZaOz2ROww56pb8GySX+r7HcochAzNq8jSJcvyzni95h0rgMX8wyEUSXOdO7h+knpW545+XqQAm6GNvcGbeuU6DN/ipJ0znPfAa91e3PbMXHz3jKfhFrGctraymiXoWA/f4h5tc4jIX+C0Yjx7rX1ep1QY3R8yOAAAAJXRFWHRkYXRlOmNyZWF0ZQAyMDE4LTEyLTE5VDIwOjEyOjExKzAxOjAwfkAm+gAAACV0RVh0ZGF0ZTptb2RpZnkAMjAxOC0xMi0xOVQyMDoxMjoxMSswMTowMA8dnkYAAAAZdEVYdFNvZnR3YXJlAHd3dy5pbmtzY2FwZS5vcmeb7jwaAAAAAElFTkSuQmCC";

        public static void ConvertToSprites()
        {
            DownloadIcon = Base64ToSprite(DownloadIconB64);
            PlayIcon = Base64ToSprite(PlayIconB64);
            ThinRingIcon = Base64ToSprite(ThinRing64);
        }

        public static Sprite Base64ToSprite(string input)
        {
            string base64 = input;
            if (input.Contains(","))
            {
                base64 = input.Substring(input.IndexOf(','));
            }
            Texture2D tex = Base64ToTexture2D(base64);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), (Vector2.one / 2f));
        }

        public static Texture2D Base64ToTexture2D(string encodedData)
        {
            byte[] imageData = Convert.FromBase64String(encodedData);

            Texture2D texture = new Texture2D(0, 0, TextureFormat.ARGB32, false, true);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Trilinear;
            texture.LoadImage(imageData);
            return texture;
        }
    }
}
