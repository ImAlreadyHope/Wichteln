import json,sys
t=json.load(open(sys.argv[1]));o=[]
th=[x['id'] for x in t['color']['themes']]
def v(x,k): return x['value'] if isinstance(x['value'],str) else x['value'].get(k,x['value'][th[0]])
for k in th:
  sel=':root, [data-theme="light"]' if k==th[0] else f'[data-theme="{k}"]'
  o.append(sel+' {'+''.join(f'--{x["name"]}:{v(x,k)};' for x in t['color']['tokens']+t['shadow']['tokens'])+'}')
dark=''.join(f'--{x["name"]}:{v(x,"dark")};' for x in t['color']['tokens']+t['shadow']['tokens'])
o.append('@media (prefers-color-scheme: dark){:root:not([data-theme="light"]){'+dark+'color-scheme:dark}}')
o.append(':root {'+''.join(f'--{x["name"]}:{x["value"]};' for x in t['spacing']['tokens']+t['radius']['tokens'])+''.join(f'--font-{k}:{s};' for k,s in t['type']['families'].items())+'}')
for f in t['type']['fonts']:
  o.append(f"@font-face{{font-family:'{f['family']}';src:url('{sys.argv[2]}{f['file'].split('/')[-1]}') format('woff2');font-weight:{f['weight']};font-display:swap}}")
print('\n'.join(o))
