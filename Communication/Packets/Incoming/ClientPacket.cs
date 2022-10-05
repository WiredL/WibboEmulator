namespace WibboEmulator.Communication.Packets.Incoming;
using System.Text;

public class ClientPacket
{
    private byte[] _body;
    private int _pointer;
    private readonly Encoding _encoding = Encoding.UTF8;

    public ClientPacket(int messageID, byte[] body) => this.Init(messageID, body);

    public int Id { get; private set; }

    public int RemainingLength => this._body.Length - this._pointer;

    public int Header => this.Id;

    public void Init(int messageID, byte[] body)
    {
        if (body == null)
        {
            body = Array.Empty<byte>();
        }

        this.Id = messageID;
        this._body = body;

        this._pointer = 0;
    }

    public byte[] ReadBytes(int bytes)
    {
        if (bytes > this.RemainingLength || bytes < 0)
        {
            bytes = this.RemainingLength;
        }

        var data = new byte[bytes];
        for (var i = 0; i < bytes; i++)
        {
            data[i] = this._body[this._pointer++];
        }

        return data;
    }

    public byte[] ReadFixedValue()
    {
        var len = 0;
        if (this.RemainingLength >= 2)
        {
            len = DecodeInt16(this.ReadBytes(2));
        }

        return this.ReadBytes(len);
    }

    public string PopString() => this._encoding.GetString(this.ReadFixedValue());

    public bool PopBoolean()
    {
        if (this.RemainingLength > 0 && this._body[this._pointer++] == Convert.ToChar(1))
        {
            return true;
        }

        return false;
    }

    public int PopInt()
    {
        if (this.RemainingLength < 4)
        {
            return 0;
        }

        var data = this.ReadBytes(4);

        var i = DecodeInt32(data);

        return i;
    }

    public override string ToString() => "[" + this.Header + "] BODY: " + this._encoding.GetString(this._body).Replace(Convert.ToChar(0).ToString(), "[0]");

    public static int DecodeInt32(byte[] v)
    {

        if ((v[0] | v[1] | v[2] | v[3]) < 0)
        {
            return 0;
        }
        return (v[0] << 24) + (v[1] << 16) + (v[2] << 8) + v[3];

    }

    public static short DecodeInt16(byte[] v)
    {
        if ((v[0] | v[1]) < 0)
        {
            return 0;
        }
        var result = (v[0] << 8) + v[1];
        return (short)result;
    }

}
