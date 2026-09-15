const fs=require('fs'),vm=require('vm'),assert=require('assert');
class Element {
 constructor(tag){this.tag=tag;this.style={};this.children=[];this.attrs={};this.events={};this.offsetTop=0;}
 appendChild(e){this.children.push(e);e.parentNode=this;return e;}
 setAttribute(k,v){this.attrs[k]=v;}
 addEventListener(k,f){this.events[k]=f;}
 removeEventListener(){}
 remove(){if(this.parentNode)this.parentNode.children=this.parentNode.children.filter(e=>e!==this);}
 getBoundingClientRect(){return {width:426,height:600};}
}
const document={head:new Element('head'),body:new Element('body'),createElement:t=>new Element(t),addEventListener(){},removeEventListener(){}};
const calls=[],window={innerWidth:1920,innerHeight:1080,addEventListener(){},removeEventListener(){}};
vm.runInNewContext(fs.readFileSync('package/IndividualSignals.js','utf8'),{document,window,engine:{trigger(...args){calls.push(args);}}});
const names=['CSKR3w1lTrafficLightCar01','CSKR3w1lTrafficLightCarCrosswalk01'];
function all(e){return [e,...e.children.flatMap(all)];}
function rows(){return all(document.body).filter(e=>e.className&&e.className.split(' ').includes('tlki-row'));}
window.tlkIndividual.show({title:'신호등 개별 편집',assets:names.map(name=>({name})),currentAsset:names[1]});
assert.equal(rows().filter(e=>e.attrs['aria-pressed']==='true').length,1);
assert.equal(rows()[1].title,names[1]);
assert(all(rows()[1]).some(e=>e.textContent==='3w1l Car Crosswalk01'));
rows()[0].events.click({stopPropagation(){}});
assert(calls.some(c=>c[0]==='tlkIndividual.choose'&&c[1]==='asset:'+names[0]));
window.tlkIndividual.show({title:'test',assets:names,currentAsset:'unknown'});
assert.equal(rows().filter(e=>e.attrs['aria-pressed']==='true').length,0);
window.tlkIndividual.show({title:'test',assets:[]});
assert.equal(rows().length,0);
window.tlkIndividual.show({title:'grid',assets:Array.from({length:7},(_,i)=>'CSKR'+i+'TrafficLightCar'),currentAsset:'CSKR6TrafficLightCar'});
const lines=all(document.body).filter(e=>e.className==='tlki-grid-line');
assert.deepEqual(lines.map(e=>e.children.length),[3,3,1]);
const img=all(rows()[0]).find(e=>e.tag==='img');
img.naturalWidth=400;img.naturalHeight=200;img.events.load();
assert.equal(img.style.width,'100px');assert.equal(img.style.height,'50px');
img.naturalWidth=100;img.naturalHeight=300;img.events.load();
assert.equal(img.style.height,'100px');
assert.equal(rows()[6].attrs['aria-pressed'],'true');
console.log('PASS: 3/3/1 grid grouping and landscape/portrait image proportions.');
const oldList=all(document.body).find(e=>e.className==='tlki-list');oldList.scrollTop=213;
document.body.children[0].scrollTop=28;
window.tlkIndividual.show({title:'grid',assets:names,currentAsset:names[0]});
assert.equal(all(document.body).find(e=>e.className==='tlki-list').scrollTop,213);
assert.equal(document.body.children[0].scrollTop,28);
assert.equal(window.tlkIndividual.move,undefined);
const header=all(document.body).find(e=>e.className==='tlki-head');
header.events.mousedown({button:0,target:header,clientX:200,clientY:150,preventDefault(){},stopPropagation(){}});
console.log('PASS: refresh retains grid and panel scroll; tracking API removed; header accepts drag.');
assert.equal(document.body.children[0].style.opacity,'.76');
assert.equal(document.body.children[0].style.transform,'scale(.96)');
function filter(label){all(document.body).find(e=>e.className&&e.className.split(' ').includes('tlki-filter')&&e.textContent===label).events.click({stopPropagation(){}});}
filter('보행자 겸용');assert.equal(rows().length,1);assert.equal(rows()[0].title,names[1]);
window.tlkIndividual.show({title:'grid',assets:names,currentAsset:names[1]});
assert.equal(rows().length,1);assert.equal(rows()[0].attrs['aria-pressed'],'true');
filter('신호등만');assert.equal(rows().length,1);assert.equal(rows()[0].title,names[0]);
all(document.body).filter(e=>e.className&&e.className.split(' ').includes('tlki-filter')&&e.textContent==='전체')[2].events.click({stopPropagation(){}});assert.equal(rows().length,2);
assert(!all(document.body).some(e=>e.textContent==='에셋 선택'||(e.textContent||'').includes('헤더를 드래그하여 이동')));
assert(all(document.body).some(e=>e.className==='tlki-grip'&&e.src==='coui://tlk/Icons/Grip.svg'));
console.log('PASS: asset filters and persistence, SVG grip, removed labels, drag feedback.');
console.log('PASS: current asset highlight, compact labels, original command IDs, unknown asset and empty list.');
// Cohtml rejects mixed percentage/pixel calc expressions even in hidden tabs.
const source=fs.readFileSync('package/IndividualSignals.js','utf8');
assert(!source.includes('calc('));assert(!source.includes('data:image'));
const signNames=['Speed30','Speed40','Speed50','Speed60','Speed70','Speed80','Speed100','StraightLeft','StraightThenStraightLeft','StraightLeftThenStraight','BanLeft','BanRight','BanStraight','BanUTurn'].map(n=>'CSKRTrafficLightSign'+n);
window.tlkIndividual.show({title:'signs',assets:names,currentAsset:names[0],mounts:[true,true,true],signs:signNames,selections:['auto','none',signNames[10]],signTab:true});
const groups=all(document.body).filter(e=>e.className==='tlki-sign-group');assert.deepEqual(groups.map(g=>g.children.length),[9,5,6]);
for(const g of groups){assert.equal(g.style.padding,'0 18px 10px');for(const b of g.children){assert.equal(b.style.width,'31.33333%');assert.equal(b.style.margin,'3px 1%');assert.equal(b.children[0].tag,'img');assert.equal(b.children[1].tag,'span');b.events.click({stopPropagation(){}});}}
for(const e of all(document.body).filter(e=>(e.src||'').startsWith('coui://tlk/Icons/'))){const svg=fs.readFileSync('package/'+e.src.slice('coui://tlk/'.length),'utf8');assert(svg.includes('<svg'));assert(!svg.includes('<text'));}
assert.equal(all(document.body).filter(e=>e.attrs['aria-checked']==='true').length,3);
assert(calls.some(c=>c[1]==='sign:3:CSKRTrafficLightSignBanUTurn'));
console.log('PASS: Cohtml layout regression, all 20 sign choices, SVG file resolution and original commands.');
window.tlkIndividual.show({title:'props',assets:names,currentAsset:names[0],propsTab:true,props:[{key:'first',name:'Attached camera',thumbnail:'camera.svg',enabled:true},{key:'second',name:'Attached box',thumbnail:'box.svg',enabled:false},{key:'pending',name:'Pending',enabled:true,pending:true}]});
const propHeader=all(document.body).find(e=>e.className==='tlki-head');
assert.equal(propHeader.children[0].className,'tlki-grip');assert.equal(propHeader.children[1].className,'tlki-heading');
const tabButtons=all(document.body).filter(e=>e.className&&e.className.split(' ').includes('tlki-tab'));assert.deepEqual(tabButtons.map(e=>e.textContent),['신호등','표지판','기타 요소']);assert.equal(tabButtons[2].attrs['aria-selected'],'true');
const propButtons=all(document.body).filter(e=>e.className==='tlki-prop');assert.equal(propButtons.length,3);
assert.equal(propButtons[0].attrs['aria-pressed'],'true');assert.equal(propButtons[1].attrs['aria-pressed'],'false');
assert(propButtons[0].children[1].src.endsWith('/PropOn.svg'));assert(propButtons[1].children[1].src.endsWith('/PropOff.svg'));
assert(propButtons.every(b=>b.children.every(e=>e.tag==='img')));
propButtons[0].events.click({stopPropagation(){}});assert(calls.some(c=>c[1]==='prop:first:0'));
propButtons[1].events.click({stopPropagation(){}});assert(calls.some(c=>c[1]==='prop:second:1'));
const callCount=calls.length;propButtons[0].events.click({stopPropagation(){}});propButtons[2].events.click({stopPropagation(){}});assert.equal(calls.length,callCount);
window.tlkIndividual.show({title:'props',assets:names,props:[]});assert.equal(all(document.body).filter(e=>e.className&&e.className.split(' ').includes('tlki-tab'))[2].attrs['aria-selected'],'true');assert(all(document.body).some(e=>e.textContent==='이 에셋에 설정된 기타 요소가 없습니다.'));
console.log('PASS: left grip, tab order/persistence, thumbnail-only toggles, green/empty icons, on/off commands and pending double-click guard.');
assert(!all(document.body).some(e=>e.attrs.role==='slider'));
window.tlkIndividual.show({title:'far',assets:names,far:true,farRotation:12,farOffset:1.5});
const controls=all(document.body).filter(e=>e.attrs.role==='slider');assert.equal(controls.length,3);assert.equal(controls[0].attrs['aria-valuenow'],'1.5');assert.equal(controls[2].attrs['aria-valuenow'],'12');assert(!all(document.body).some(e=>e.tag==='input'));
const evt={key:'ArrowRight',preventDefault(){},stopPropagation(){}};controls[0].events.keydown(evt);assert(calls.some(c=>c[1]==='offset:1.6'));controls[2].events.keydown(evt);assert(calls.some(c=>c[1]==='rotation:13'));
controls[2].events.keydown({key:'End',preventDefault(){},stopPropagation(){}});assert(calls.some(c=>c[1]==='rotation:45'));
const resets=all(document.body).filter(e=>e.className==='tlki-control-reset');resets[2].events.click({stopPropagation(){}});assert(calls.some(c=>c[1]==='rotation:0'));
const beforeMouse=calls.length;controls[0].getBoundingClientRect=()=>({left:10,width:200});controls[0].events.mousedown({button:0,clientX:110,preventDefault(){},stopPropagation(){}});assert.equal(calls.length,beforeMouse);assert.equal(controls[0].attrs['aria-valuenow'],'0.0');
for(const name of ['Move','Rotate','Reset','Minus','Plus'])assert(fs.readFileSync('package/Icons/'+name+'.svg','utf8').includes('<svg'));
window.tlkIndividual.show({title:'near',assets:names,far:false});assert(!all(document.body).some(e=>e.attrs.role==='slider'));
console.log('PASS: custom sliders, no native input, keyboard bounds, independent commands, drag preview, reset and SVG assets.');controls[1].events.keydown({key:'ArrowRight',preventDefault(){},stopPropagation(){}});assert(calls.some(c=>c[1]==='lateral:0.1'));
const filterNames=['CSKR3w2lLeftTrafficLightCar01','CSKR4w1lTrafficLightCarLeft01','CSKR4w2lTrafficLightCarCrosswalk01'];
window.tlkIndividual.show({title:'filters',assets:filterNames});
function chooseFilter(label){const f=all(document.body).find(e=>e.className&&e.className.split(' ').includes('tlki-filter')&&e.textContent===label);assert(f);f.events.click({stopPropagation(){}});}
assert.deepEqual(all(document.body).filter(e=>e.className&&e.className.split(' ').includes('tlki-filter')).map(e=>e.textContent),['전체','3구','4구','전체','좌 기둥','우 기둥','전체','신호등만','보행자 겸용']);
chooseFilter('좌 기둥');assert.equal(rows().length,1);assert.equal(rows()[0].title,filterNames[1]);chooseFilter('우 기둥');assert.equal(rows().length,2);chooseFilter('3구');assert.equal(rows().length,1);assert.equal(rows()[0].title,filterNames[0]);chooseFilter('4구');assert.equal(rows().length,1);assert.equal(rows()[0].title,filterNames[2]);chooseFilter('좌 기둥');assert.equal(rows().length,1);assert.equal(rows()[0].title,filterNames[1]);chooseFilter('보행자 겸용');assert.equal(rows().length,0);
console.log('PASS: lateral command and 3w/4w/Left01/complement filters.');window.tlkIndividual.show({title:'fold',assets:filterNames,far:true});let fold=all(document.body).find(e=>e.className==='tlki-placement-toggle');assert.equal(fold.attrs['aria-expanded'],'false');fold.events.click({stopPropagation(){}});assert.equal(fold.attrs['aria-expanded'],'true');window.tlkIndividual.show({title:'fold',assets:filterNames,far:true});assert.equal(all(document.body).find(e=>e.className==='tlki-placement-toggle').attrs['aria-expanded'],'true');
console.log('PASS: intersection of three independent filters and collapsed placement persistence.');
function actionNamed(label){return all(document.body).find(e=>e.tag==='button'&&all(e).some(n=>n.textContent===label));}
window.tlkIndividual.show({title:'near',assets:names,far:false,farReady:false});assert(!actionNamed('본 위치 신호등만 숨기기'));
window.tlkIndividual.show({title:'near',assets:names,far:false,farReady:true});actionNamed('본 위치 신호등만 숨기기').events.click({stopPropagation(){}});assert(calls.some(c=>c[1]==='hideNear'));
window.tlkIndividual.show({title:'far',assets:names,far:true,nearHidden:true});actionNamed('본 위치 신호등 살리기').events.click({stopPropagation(){}});assert(calls.some(c=>c[1]==='restoreNear'));
window.tlkIndividual.show({title:'far',assets:names,far:true,nearHidden:false});assert(!actionNamed('본 위치 신호등 살리기'));
console.log('PASS: original hide requires far display, restoration is offered on the far signal.');
const roadNames=['CSKRRoadNameSignBoth','CSKRRoadNameSignLeft','CSKRRoadNameSignRight'];
window.tlkIndividual.show({title:'road names',assets:names,propsTab:true,roadNames:roadNames.map(name=>({name,thumbnail:'test.svg'})),roadName:roadNames[1],roadNameOriginal:false});
const roadButtons=all(document.body).filter(e=>e.tag==='button'&&roadNames.includes(e.title));assert.equal(roadButtons.length,3);assert.equal(roadButtons[1].attrs['aria-checked'],'true');assert.equal(roadButtons[0].attrs['aria-checked'],'false');roadButtons[2].events.click({stopPropagation(){}});assert(calls.some(c=>c[1]==='roadname:CSKRRoadNameSignRight'));actionNamed('원래 구성으로 되돌리기').events.click({stopPropagation(){}});assert(calls.some(c=>c[1]==='roadname:'));
window.tlkIndividual.show({title:'no mount',assets:names,propsTab:true,roadNames:[]});assert(!actionNamed('원래 구성으로 되돌리기'));console.log('PASS: three road-name variants, active selection, swap/reset commands and absent mount.');
