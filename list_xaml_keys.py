import re
import os
import glob
root = 'WYSHUB'
static_keys = {}
def_keys = {}
resource_files = []
for path in glob.glob(os.path.join(root, '**', '*.xaml'), recursive=True):
    with open(path, encoding='utf-8') as f:
        txt = f.read()
    if '<ResourceDictionary' in txt:
        resource_files.append(path)
    for m in re.finditer(r'\{StaticResource\s+([^\}\s]+)\}', txt):
        k = m.group(1).strip()
        static_keys.setdefault(k, []).append(path)
    for m in re.finditer(r'x:Key\s*=\s*"([^"]+)"', txt):
        k = m.group(1).strip()
        def_keys.setdefault(k, []).append(path)
app = os.path.join(root, 'App.xaml')
with open(app, encoding='utf-8') as f:
    apptxt = f.read()
md = re.findall(r'<ResourceDictionary\s+Source\s*=\s*"([^"]+)"', apptxt)
print('App.xaml merged dictionaries:')
for m in md:
    print(' -', m)
print('\nResourceDictionary root files:')
for p in sorted(resource_files):
    print(' -', p)
print('\nDefined keys count:', len(def_keys))
for key in sorted(def_keys):
    print(key)
print('\nStaticResource keys used count:', len(static_keys))
for key in sorted(static_keys):
    print(key)
missing = [k for k in sorted(static_keys) if k not in def_keys]
print('\nMissing keys count:', len(missing))
for key in missing:
    print(key)
