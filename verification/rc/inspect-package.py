"""Inspect local RC contents without installation, publication, or assembly execution."""
from pathlib import Path
import hashlib, json, zipfile, xml.etree.ElementTree as ET

repo = Path(__file__).resolve().parents[2]
library = repo / 'IVSoftware.Portable.SQLiteMarkdown'
package = library / 'bin/Release/IVSoftware.Portable.SQLiteMarkdown.2.0.0-rc.nupkg'
with zipfile.ZipFile(package) as archive:
    entries = archive.namelist()
    assert not any(name.lower().endswith(('.pfx', '.snk', '.key')) for name in entries)
    assembly = archive.read('lib/netstandard2.0/IVSoftware.Portable.SQLiteMarkdown.dll')
    assert assembly == (library / 'bin/Release/netstandard2.0/IVSoftware.Portable.SQLiteMarkdown.dll').read_bytes()
    assert b'SQLitePrismaConnection' in assembly
    assert b'ObservableQueryFilterCollection' not in assembly
    assert b'TemporalAffinity' not in assembly
    nuspec = ET.fromstring(archive.read('IVSoftware.Portable.SQLiteMarkdown.nuspec'))
    namespace = {'n': 'http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd'}
    assert nuspec.find('n:metadata/n:version', namespace).text == '2.0.0-rc'
    dependencies = [item.attrib for item in nuspec.findall('.//n:dependency', namespace)]
    assert not any('Collections' in item['id'] or 'Affinity' in item['id'] for item in dependencies)
    assert 'SQLitePrismaConnection' in archive.read('README.md').decode('utf-8-sig')

preserved = json.loads((repo / 'verification/rc/pre-integration/manifest.json').read_text())
for entry in preserved:
    archived = repo / 'verification/rc/pre-integration' / entry['path']
    assert hashlib.sha256(archived.read_bytes()).hexdigest() == entry['sha256']

report = {'package': str(package), 'sha256': hashlib.sha256(package.read_bytes()).hexdigest(),
          'entries': entries, 'dependencies': dependencies, 'preserved_source_files': len(preserved),
          'no_signing_keys': True, 'contains_connection_facade': True,
          'no_modeled_collection_or_temporal_implementation': True,
          'package_matches_release_assembly': True, 'publication': False, 'assembly_signing': False}
(repo / 'verification/rc/package-final.json').write_text(json.dumps(report, indent=2))
print(json.dumps(report, indent=2))
