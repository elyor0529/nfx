/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2018 Agnicore Inc. portions ITAdapter Corp. Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
/*
 * Author: Dmitriy Khmaladze, Spring 2015  dmitriy@itadapter.com
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.DataAccess.Distributed;
using NFX.IO.ErrorHandling;

namespace NFX.ApplicationModel.Pile
{
  /// <summary>
  /// Represents a value-type GDID+3 char ISO code tuple, suitable for serving as a cache table key.
  /// The point of this structure is to avoid creation of references for Pile cache so keys alone do not stall the GC.
  /// The ISO code is case-insensitive
  /// </summary>
  public struct GDIDWithISOKey : IDistributedStableHashProvider, IEquatable<GDIDWithISOKey>
  {
    public GDIDWithISOKey(GDID gdid, string iso)
    {
      GDID = gdid;
      ISO = IOMiscUtils.PackISO3CodeToInt(iso);
    }

    public readonly GDID GDID;
    public readonly int ISO;


    public string ISOCode { get{ return IOMiscUtils.UnpackISO3CodeFromInt(ISO);} }


    public ulong GetDistributedStableHash()
    {
      return GDID.GetDistributedStableHash() ^ (ulong)ISO;
    }

    public override int GetHashCode()
    {
      return GDID.GetHashCode() ^ ISO;
    }

    public bool Equals(GDIDWithISOKey other)
    {
      return this.ISO  == other.ISO &&
             this.GDID == other.GDID;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is GDIDWithISOKey)) return false;
      return this.Equals((GDIDWithISOKey)obj);
    }

    public override string ToString()
    {
      return "Key[GDID: '{0}' ISO: '{1}']".Args(GDID, ISOCode);
    }
  }


  /// <summary>
  /// Represents a date (not time)-sensitive value-type GDID+3 char ISO code tuple, suitable for serving as a cache table key.
  /// The point of this structure is to avoid creation of references for Pile cache so keys alone do not stall the GC.
  /// The ISO code is case-insensitive
  /// </summary>
  public struct DatedGDIDWithISOKey : IDistributedStableHashProvider, IEquatable<DatedGDIDWithISOKey>
  {

    internal static int DateToYMD(DateTime dt)
    {
      return ((dt.Year  & 0xffff) << 16) +
             ((dt.Month & 0xff)  << 8) +
             ((dt.Day   & 0xff));
    }

    internal static DateTime YMDtoDate(int ymd)
    {
      var result = new DateTime(((ymd >> 16) & 0xffff), ((ymd >> 8) & 0xff), (ymd & 0xff), 0, 0, 0, DateTimeKind.Utc);
      return result;
    }


    public DatedGDIDWithISOKey(DateTime date, GDID gdid, string iso)
    {
      GDID = gdid;
      ISO = IOMiscUtils.PackISO3CodeToInt(iso);
      YMD = DatedGDIDWithISOKey.DateToYMD(date);
    }

    public readonly int  YMD;
    public readonly GDID GDID;
    public readonly int  ISO;


    public string ISOCode { get{ return IOMiscUtils.UnpackISO3CodeFromInt(ISO);} }

    public DateTime DateTime { get{ return YMDtoDate(YMD);} }


    public ulong GetDistributedStableHash()
    {
      return GDID.GetDistributedStableHash() ^ (((ulong)ISO)<<32) ^ (ulong)YMD;
    }

    public override int GetHashCode()
    {
      return GDID.GetHashCode() ^ ISO ^ YMD;
    }

    public bool Equals(DatedGDIDWithISOKey other)
    {
      return this.YMD  == other.YMD &&
             this.ISO  == other.ISO &&
             this.GDID == other.GDID;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is DatedGDIDWithISOKey)) return false;
      return this.Equals((DatedGDIDWithISOKey)obj);
    }

    public override string ToString()
    {
      return "Key[GDID: '{0}' ISO: '{1}' Date: '{2:yyyy-MM-dd}']".Args(GDID, ISOCode, DateTime);
    }
  }


  /// <summary>
  /// Represents a date (not time)-sensitive value-type 2 GDIDs+3 char ISO code tuple, suitable for serving as a cache table key.
  /// The point of this structure is to avoid creation of references for Pile cache so keys alone do not stall the GC.
  /// The ISO code is case-insensitive
  /// </summary>
  public struct Dated2GDIDWithISOKey : IDistributedStableHashProvider, IEquatable<Dated2GDIDWithISOKey>
  {

    public Dated2GDIDWithISOKey(DateTime date, GDID gdid1, GDID gdid2, string iso)
    {
      GDID1 = gdid1;
      GDID2 = gdid2;
      ISO = IOMiscUtils.PackISO3CodeToInt(iso);
      YMD = DatedGDIDWithISOKey.DateToYMD(date);
    }

    public readonly int  YMD;
    public readonly GDID GDID1;
    public readonly GDID GDID2;
    public readonly int  ISO;


    public string ISOCode { get{ return IOMiscUtils.UnpackISO3CodeFromInt(ISO);} }

    public DateTime DateTime { get{ return DatedGDIDWithISOKey.YMDtoDate(YMD);} }


    public ulong GetDistributedStableHash()
    {
      return GDID1.GetDistributedStableHash() ^
             GDID2.GetDistributedStableHash() ^
             (((ulong)ISO)<<32) ^ (ulong)YMD;
    }

    public override int GetHashCode()
    {
      return GDID1.GetHashCode() ^ GDID2.GetHashCode() ^ ISO ^ YMD;
    }

    public bool Equals(Dated2GDIDWithISOKey other)
    {
      return this.YMD  == other.YMD &&
             this.ISO  == other.ISO &&
             this.GDID1 == other.GDID1 &&
             this.GDID2 == other.GDID2 ;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is Dated2GDIDWithISOKey)) return false;
      return this.Equals((Dated2GDIDWithISOKey)obj);
    }

    public override string ToString()
    {
      return "Key[GDID1: '{0}' GDID2: '{1}' ISO: '{2}' Date: '{3:yyyy-MM-dd}']".Args(GDID1, GDID2, ISOCode, DateTime);
    }
  }


  /// <summary>
  /// Represents a value-type 2 GDIDs+3 char ISO code tuple, suitable for serving as a cache table key.
  /// The point of this structure is to avoid creation of references for Pile cache so keys alone do not stall the GC.
  /// The ISO code is case-insensitive
  /// </summary>
  public struct TwoGDIDWithISOKey : IDistributedStableHashProvider, IEquatable<TwoGDIDWithISOKey>
  {

    public TwoGDIDWithISOKey(GDID gdid1, GDID gdid2, string iso)
    {
      GDID1 = gdid1;
      GDID2 = gdid2;
      ISO = IOMiscUtils.PackISO3CodeToInt(iso);
    }

    public readonly GDID GDID1;
    public readonly GDID GDID2;
    public readonly int  ISO;


    public string ISOCode { get{ return IOMiscUtils.UnpackISO3CodeFromInt(ISO);} }


    public ulong GetDistributedStableHash()
    {
      return GDID1.GetDistributedStableHash() ^
             GDID2.GetDistributedStableHash() ^
             (((ulong)ISO)<<32);
    }

    public override int GetHashCode()
    {
      return GDID1.GetHashCode() ^ GDID2.GetHashCode() ^ ISO;
    }

    public bool Equals(TwoGDIDWithISOKey other)
    {
      return this.ISO  == other.ISO &&
             this.GDID1 == other.GDID1 &&
             this.GDID2 == other.GDID2 ;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is TwoGDIDWithISOKey)) return false;
      return this.Equals((TwoGDIDWithISOKey)obj);
    }

    public override string ToString()
    {
      return "Key[GDID1: '{0}' GDID2: '{1}' ISO: '{2}']".Args(GDID1, GDID2, ISOCode);
    }
  }

  /// <summary>
  /// Represents a GDID with 8 byte string hash. The hash is case sensitive.
  /// The point of this structure is to avoid creation of references for Pile cache so keys alone do not stall the GC.
  /// </summary>
  public struct GDIDWithStrHash : IDistributedStableHashProvider, IEquatable<GDIDWithStrHash>
  {
    public static ulong StrToHash(string key)
    {
      //WARNING! Never use GetHashCode here as it is platform-dependent, but this function must be 100% deterministic

      if (key == null) return 0;
      var sl = key.Length;
      if (sl == 0) return 0;

      ulong hash1 = 0;
      for (int i = sl - 1; i > sl - 1 - sizeof(ulong) && i >= 0; i--)//take 8 chars from end (string suffix), for most string the
      {                                                              //string tail is the most changing part (i.e. 'Alex Kozloff'/'Alex Richardson'/'System.A'/'System.B'
        if (i < sl - 1) hash1 <<= 8;
        var c = key[i];
        var b1 = (c & 0xff00) >> 8;
        var b2 = c & 0xff;
        hash1 |= (byte)(b1 ^ b2);
      }

      ulong hash2 = 1566083941ul * (ulong)Adler32.ForString(key);

      return hash1 ^ hash2;
    }


    public GDIDWithStrHash(GDID gdid, string key)
    {
      GDID = gdid;
      Hash = GDIDWithStrHash.StrToHash(key);
    }

    public readonly GDID GDID;
    public readonly ulong Hash;


    public ulong GetDistributedStableHash()
    {
      return GDID.GetDistributedStableHash() ^ Hash;
    }

    public override int GetHashCode()
    {
      return GDID.GetHashCode() ^ Hash.GetHashCode();
    }

    public bool Equals(GDIDWithStrHash other)
    {
      return this.Hash == other.Hash &&
             this.GDID == other.GDID;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is GDIDWithStrHash)) return false;
      return this.Equals((GDIDWithStrHash)obj);
    }

    public override string ToString()
    {
      return "Key[GDID: '{0}' Hash: '{1}']".Args(GDID, Hash);
    }
  }

  /// <summary>
  /// Represents GDID with int
  /// </summary>
  public struct GDIDWithInt : IDistributedStableHashProvider, IEquatable<GDIDWithInt>
  {
    public GDIDWithInt(GDID gdid, int val)
    {
      GDID = gdid;
      Int = val;
    }

    public readonly GDID GDID;
    public readonly int Int;


    public ulong GetDistributedStableHash()
    {
      return GDID.GetDistributedStableHash() ^ (ulong)Int;
    }

    public override int GetHashCode()
    {
      return GDID.GetHashCode() ^ Int.GetHashCode();
    }

    public bool Equals(GDIDWithInt other)
    {
      return this.Int == other.Int &&
             this.GDID == other.GDID;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is GDIDWithInt)) return false;
      return this.Equals((GDIDWithInt)obj);
    }

    public override string ToString()
    {
      return "Key[GDID: '{0}' Int: '{1}']".Args(GDID, Int);
    }
  }

}
