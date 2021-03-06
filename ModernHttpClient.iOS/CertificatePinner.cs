﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

namespace ModernHttpClient
{
    public class CertificatePinner
    {
        private readonly Dictionary<string, string[]> Pins;

        public CertificatePinner()
        {
            Pins = new Dictionary<string, string[]>();
        }

        public bool HasPins(string hostname)
        {
            foreach(var pin in Pins)
            {
                if (Utility.MatchHostnameToPattern(hostname, pin.Key))
                {
                    return true;
                }
            }

            return false;
        }

        public void AddPins(string hostname, string[] pins)
        {
            Utility.VerifyPins(pins);
            Pins[hostname] = pins;
        }

        public bool Check(string hostname, byte[] certificate)
        {
            if (!HasPins(hostname))
            {
                Debug.WriteLine($"No certificate pin found for {hostname}");
                return false;
            }

            hostname = Pins.FirstOrDefault(p => Utility.MatchHostnameToPattern(hostname, p.Key)).Key;

            // Get pins
            string[] pins = Pins[hostname];

            // Skip pinning with empty array
            if (pins == null || pins.Length == 0)
            {
                return true;
            }

            // Compute spki fingerprint
            var spkiFingerprint = SpkiFingerprint.Compute(certificate);

            // Check pin
            var match = Array.IndexOf(pins, spkiFingerprint) > -1;

            if (match)
            {
                Debug.WriteLine($"Certificate pin is ok for {hostname}");
            }
            else
            {
                Debug.WriteLine($"Certificate pinning failure! Peer certificate chain: {spkiFingerprint}, Pinned certificates for {hostname}: {string.Join("|", pins)}");
            }

            return match;
        }
    }
}
