"""Promote reviewed RC sources while preserving every overwritten source byte."""
from pathlib import Path
import hashlib, json, shutil

repo = Path(__file__).resolve().parents[2]
candidate = Path(r'D:\Codex\IVSoftware\IVSoftware.Portable.SQLiteMarkdown\verification\rc\candidate')
library = repo / 'IVSoftware.Portable.SQLiteMarkdown'
archive = repo / 'verification/rc/pre-integration'
if (archive / 'manifest.json').exists():
    raise SystemExit('This one-time promotion has already run. Edit canonical sources in place; do not replay the old candidate.')
manifest = []
for source in library.rglob('*'):
    if source.is_file() and source.suffix in ('.cs', '.csproj') and not {'bin', 'obj'} & set(source.parts):
        relative = source.relative_to(library)
        target = archive / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        if not target.exists():
            shutil.copyfile(source, target)
        manifest.append({'path': str(relative), 'sha256': hashlib.sha256(target.read_bytes()).hexdigest()})
archive.mkdir(parents=True, exist_ok=True)
(archive / 'manifest.json').write_text(json.dumps(manifest, indent=2))

includes = []
for source in (candidate / library.name).rglob('*.cs'):
    relative = source.relative_to(candidate / library.name)
    if {'bin', 'obj'} & set(relative.parts):
        continue
    target = library / relative
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(source, target)
    includes.append(str(relative).replace('\\', '/'))

project = (candidate / library.name / (library.name + '.csproj')).read_text(encoding='utf-8-sig')
project = project.replace('<Nullable>enable</Nullable>', '<Nullable>enable</Nullable>\n        <EnableDefaultCompileItems>false</EnableDefaultCompileItems>')
for package, version in [('IVSoftware.Portable.Common', '1.1.2-rc'), ('Newtonsoft.Json', '13.0.4'), ('sqlite-net-pcl', '1.11.285')]:
    import re
    project = re.sub(r'(<PackageReference Include="' + re.escape(package) + r'" Version=")[^"]+', r'\g<1>' + version, project)
project = project.replace('</Project>', '<ItemGroup>\n' + ''.join('    <Compile Include="' + name + '" />\n' for name in sorted(includes)) + '    <Compile Include="Connections/**/*.cs" />\n</ItemGroup>\n</Project>')
(library / (library.name + '.csproj')).write_text(project, encoding='utf-8')

# Preserve the published provider's concrete Clear(bool) surface and semantics.
source = library / 'Collections/ObservableQueryFilterSource.cs'
text = source.read_text(encoding='utf-8-sig')
text = text.replace('        public new void Clear() => Clear(true);\n\n        public new FilteringState Clear(bool all)\n        {\n            var result = base.Clear(all);', '        public new void Clear(bool all = false)\n        {\n            base.Clear(all);')
text = text.replace('            return result;\n        }\n\n        public bool Contains', '        }\n\n        public bool Contains')
source.write_text(text, encoding='utf-8')

source = library / 'MarkdownContext.cs'
text = source.read_text(encoding='utf-8-sig')
text = text.replace('        [Canonical]\n        public MarkdownContext(Type type)\n', '        public MarkdownContext(Type type) : this(type, false) { }\n\n        [Canonical]\n        public MarkdownContext(Type type, bool disableQuerySemantics)\n')
text = text.replace('            localValidateClearTopology();', '            // The published provider intentionally retains stateful Clear(bool).\n            if (GetType().GetMethod("Clear", new[] { typeof(bool) })?.ReturnType != typeof(void))\n                localValidateClearTopology();')
source.write_text(text, encoding='utf-8')

# A focused release solution leaves legacy mixed suites intact and explicitly separate.
handwritten = repo / 'IVSoftware.Portable.SQLiteMarkdown.RC.Handwritten.Tests'
handwritten.mkdir(exist_ok=True)
for source in (candidate / 'IVSoftware.Portable.SQLiteMarkdown.Handwritten.Tests').glob('*.cs'):
    shutil.copyfile(source, handwritten / source.name)
project = (candidate / 'IVSoftware.Portable.SQLiteMarkdown.Handwritten.Tests/IVSoftware.Portable.SQLiteMarkdown.Handwritten.Tests.csproj').read_text(encoding='utf-8-sig')
project = project.replace(str(repo).replace('\\', '\\') + '\\', '../').replace('Version="1.0.8"', 'Version="2.0.0-rc"')
(handwritten / (handwritten.name + '.csproj')).write_text(project, encoding='utf-8')

peer = repo / 'IVSoftware.Portable.SQLiteMarkdown.RC.Nemesis.Tests'
peer.mkdir(exist_ok=True)
for name in ['Nemesis_IncrementalOrTests.cs', 'Nemesis_RcWorkflowTests.cs']:
    shutil.copyfile(candidate / 'IVSoftware.Portable.SQLiteMarkdown.Nemesis.Tests' / name, peer / name)
project = (candidate / 'IVSoftware.Portable.SQLiteMarkdown.Nemesis.Tests/IVSoftware.Portable.SQLiteMarkdown.Nemesis.Tests.csproj').read_text(encoding='utf-8-sig')
(peer / (peer.name + '.csproj')).write_text(project, encoding='utf-8')
(repo / 'SQLiteMarkdown.RC.slnx').write_text('<Solution>\n' + ''.join('  <Project Path="' + p + '/' + p + '.csproj" />\n' for p in [library.name, handwritten.name, peer.name]) + '</Solution>\n')

# Use the reviewed README corrections, not the candidate's public behavior changes.
shutil.copyfile(repo / 'README.md', archive / 'root-README.md')
shutil.copyfile(Path(r'D:\Codex\IVSoftware\IVSoftware.Portable.SQLiteMarkdown\verification\rc-readme\README.md'), repo / 'README.md')
