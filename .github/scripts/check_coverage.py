#!/usr/bin/env python3
"""Gộp các file cobertura của job unit và kiểm tra ngưỡng line coverage Domain + Application (NFR-16)."""
import glob
import sys
import xml.etree.ElementTree as ET

threshold = float(sys.argv[2])
lines = {}
for path in glob.glob(sys.argv[1] + "/**/coverage.cobertura.xml", recursive=True):
    for package in ET.parse(path).getroot().iter("package"):
        for cls in package.iter("class"):
            for line in cls.iter("line"):
                key = (package.get("name"), cls.get("filename"), line.get("number"))
                lines[key] = max(lines.get(key, 0), int(line.get("hits")))

if not lines:
    print("Không tìm thấy dữ liệu coverage")
    sys.exit(1)

covered = sum(1 for hits in lines.values() if hits > 0)
percent = covered * 100 / len(lines)
print(f"Line coverage Domain + Application: {percent:.1f}% ({covered}/{len(lines)}), ngưỡng {threshold:.0f}%")
if percent < threshold:
    print("Coverage dưới ngưỡng")
    sys.exit(1)
