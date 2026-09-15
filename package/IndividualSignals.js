(function () {
  if (window.tlkIndividual) { return; }
  var panel = null, x = 300, y = 220, position = null, drag = null, activeList = null, assetFilter = 'all', lampFilter='all', poleFilter='all', placementOpen=false, activeTab = 0;
  var css = document.createElement('style');
  css.textContent = [
    '.tlki-panel{position:fixed;z-index:99999;width:386px;max-height:78vh;overflow:auto;background:rgba(13,14,16,.91);color:#eeeeef;border:1px solid rgba(255,255,255,.16);border-radius:14px;box-shadow:0 16px 48px rgba(0,0,0,.48);font-family:var(--fontFamily);font-size:14px;pointer-events:auto;box-sizing:border-box;}',
    '.tlki-panel,.tlki-panel *{font-family:"Noto Sans KR";box-sizing:border-box}.tlki-head{display:flex;align-items:center;padding:18px;border-bottom:1px solid rgba(255,255,255,.09)}',
    '.tlki-heading{flex:1;min-width:0}.tlki-eyebrow{font-size:9px;letter-spacing:1.5px;color:#929397;margin-bottom:6px}.tlki-title{font-size:19px;font-weight:600;line-height:1.3}',
    '.tlki-tab{width:33.33333%;padding:10px;border:0;border-radius:6px;background:transparent}.tlki-panel button{font-family:var(--fontFamily);cursor:pointer;color:inherit}.tlki-close{display:flex;align-items:center;justify-content:center;width:32px;height:32px;flex-shrink:0;margin-left:12px;padding:7px;border-radius:8px;border:1px solid rgba(244,92,99,.4);background:rgba(183,38,47,.26)}',
    '.tlki-close:hover{background:rgba(220,55,65,.5);border-color:#f4757b}.tlki-close:focus{outline:2px solid #ff9297;outline-offset:2px}',
    '.tlki-section{display:flex;align-items:center;padding:15px 18px 9px;color:#b6b7ba;font-size:11px;letter-spacing:.5px}.tlki-count{margin-left:8px;padding:2px 6px;border-radius:5px;background:rgba(255,255,255,.08);color:#d4d4d6}',
    '.tlki-list{max-height:268px;overflow:auto;padding:0 10px 8px}.tlki-row{display:flex;align-items:center;width:100%;min-height:64px;padding:8px;text-align:left;background:transparent;border:0;border-bottom:1px solid rgba(255,255,255,.07);border-radius:5px}',
    '.tlki-row:hover,.tlki-row:focus{background:rgba(255,255,255,.09);outline:none}.tlki-row:focus{box-shadow:inset 0 0 0 1px rgba(255,255,255,.4)}',
    '.tlki-thumb{display:flex;align-items:center;justify-content:center;width:46px;height:46px;flex-shrink:0;margin-right:12px;border-radius:8px;background:rgba(255,255,255,.055);border:1px solid rgba(255,255,255,.09)}.tlki-thumb img{width:38px;height:38px;object-fit:contain}',
    '.tlki-name{flex:1;min-width:0;white-space:normal;word-break:break-all;line-height:1.45;font-size:13px}.tlki-arrow{margin-left:10px;opacity:.35}.tlki-row:hover .tlki-arrow{opacity:.9}',
    '.tlki-actions{padding:9px 10px;border-top:1px solid rgba(255,255,255,.1);background:rgba(0,0,0,.16)}.tlki-action{display:flex;align-items:center;width:100%;padding:10px 9px;margin:2px 0;border:1px solid transparent;border-radius:7px;background:transparent;text-align:left;font-size:13px}',
    '.tlki-action:hover,.tlki-action:focus{background:rgba(255,255,255,.075);border-color:rgba(255,255,255,.1);outline:none}.tlki-action>span:first-child{margin-right:11px;flex-shrink:0}.tlki-danger{color:#f19498!important}.tlki-danger:hover{background:rgba(214,58,66,.12)}',
    '.tlki-note{display:flex;padding:12px 18px 16px;color:#939499;font-size:11px;line-height:1.55;border-top:1px solid rgba(255,255,255,.07)}.tlki-note>span:first-child{margin-right:9px;margin-top:2px;flex-shrink:0}.tlki-status,.tlki-empty{margin:10px 18px;color:#c3c3c6;font-size:12px;line-height:1.5}',
    '.tlki-panel::-webkit-scrollbar,.tlki-list::-webkit-scrollbar{width:5px}.tlki-panel::-webkit-scrollbar-thumb,.tlki-list::-webkit-scrollbar-thumb{background:rgba(255,255,255,.22);border-radius:4px}'
    ,'.tlki-panel{width:426px;max-width:98vw}.tlki-list{max-height:350px;padding:4px 10px 8px}.tlki-row{position:relative;flex-direction:column;width:48%;min-height:168px;margin:4px;padding:9px;border:1px solid rgba(255,255,255,.15);border-radius:10px;background:rgba(255,255,255,.035)}',
    '.tlki-thumb{width:100%;height:104px;margin:0 0 8px;border:0;border-radius:6px;background:rgba(0,0,0,.18)}.tlki-thumb img{width:100%;height:100px;object-fit:contain}.tlki-name{flex:none;width:100%;font-size:12px;text-align:center;word-break:normal;overflow-wrap:break-word}.tlki-row.tlki-selected{border:2px solid #83d6fb;padding:8px;background:rgba(50,142,187,.25);box-shadow:inset 0 0 0 1px rgba(131,214,251,.15)}',
    '.tlki-current{position:absolute;top:5px;right:5px;padding:3px 6px;border-radius:4px;background:#245d76;font-size:10px;font-weight:600}.tlki-current-summary{padding:0 18px 8px;font-size:12px;line-height:1.5;color:#a9dff5}.tlki-row:focus{outline:2px solid #d2efff;outline-offset:1px}'
    ,'.tlki-list{display:flex;flex-direction:column;flex-wrap:nowrap;align-items:flex-start;overflow-x:hidden;overflow-y:auto;padding:4px 10px 8px}.tlki-grid-line{display:flex;flex-direction:row;flex-wrap:nowrap;flex-shrink:0;width:396px;min-height:170px;align-items:stretch}.tlki-grid-line .tlki-row{display:flex;flex-direction:column;flex-grow:0;flex-shrink:0;width:124px;min-width:124px;max-width:124px;min-height:162px;margin:4px;padding:10px;border-width:1px}.tlki-grid-line .tlki-thumb{width:100px;height:100px;min-width:100px;min-height:100px;max-width:100px;max-height:100px;flex-shrink:0;margin:0 0 8px;overflow:hidden}.tlki-grid-line .tlki-thumb img{width:100px;height:100px;flex-shrink:0}.tlki-grid-line .tlki-name{width:100px;font-size:11px;word-break:break-all}.tlki-grid-line .tlki-selected{border-width:1px;padding:10px;box-shadow:inset 0 0 0 1px #83d6fb}'
    ,'.tlki-head{cursor:move;background:rgba(35,43,51,.96);user-select:none}.tlki-drag-hint{font-size:10px;color:#aebfca;margin-top:5px}.tlki-panel{background:rgba(13,18,23,.96)}'
    ,'.tlki-tabs{display:flex;margin:8px 18px 0;border-bottom:1px solid rgba(160,183,202,.22)}.tlki-tabs .tlki-tab{width:33.33333%;padding:14px 4px 12px;border:0;border-bottom:3px solid transparent;border-radius:0;background:transparent;font-size:13px;font-weight:500;line-height:1.4;text-align:center}.tlki-tabs .tlki-tab:hover{background-color:rgba(83,163,225,.08)}.tlki-tabs .tlki-tab:focus{outline:1px solid #83caff;outline-offset:-3px}.tlki-tabs .tlki-tab-active{border-bottom-color:#55b8ff;background:rgba(55,150,225,.12);font-weight:700}'
    ,'.tlki-head{position:relative;padding:18px}.tlki-grip{width:16px;height:24px;margin-right:14px;flex-shrink:0;pointer-events:none}.tlki-filters{display:flex;flex-direction:row;padding:8px 14px}.tlki-filter{flex:1;padding:8px 3px;margin:0 2px;border:1px solid rgba(255,255,255,.15);border-radius:6px;background:rgba(255,255,255,.035);font-size:11px}.tlki-filter:hover{background:rgba(130,195,230,.17)}.tlki-filter-active{background:rgba(60,142,187,.35);border-color:#83d6fb}.tlki-sign-choice:hover{background-color:rgba(85,165,210,.32)!important;border-color:#a5ddfa!important;box-shadow:inset 0 0 0 1px rgba(165,221,250,.5)}.tlki-sign-choice:focus{outline:2px solid #a5ddfa;outline-offset:1px}'
    ,'.tlki-prop:hover{background-color:rgba(100,165,195,.2)!important}.tlki-prop:focus{outline:2px solid #a5ddfa;outline-offset:1px}.tlki-prop:disabled{cursor:wait}'
  ].join('\n');
  document.head.appendChild(css);
  css.textContent += ".tlki-transform-controls{padding:12px 18px}.tlki-control-card{padding:14px;margin-bottom:10px;border:1px solid #3b4c5b;border-radius:9px;background:#202c37}.tlki-control-head{display:flex;align-items:center;margin-bottom:10px}.tlki-control-icon{width:18px;height:18px;flex-shrink:0}.tlki-control-title{margin-left:8px;font-size:12px;flex:1}.tlki-control-value{font-size:13px;color:#8ed7ff!important;margin-right:10px;min-width:58px;text-align:right}.tlki-control-reset,.tlki-nudge{display:flex;align-items:center;justify-content:center;width:28px;height:28px;padding:5px;border:1px solid #526879;border-radius:5px;background:#293c4c;flex-shrink:0}.tlki-control-reset:hover,.tlki-nudge:hover{background:#365973}.tlki-control-row{display:flex;align-items:center}.tlki-slider{position:relative;flex:1;height:32px;margin:0 12px;cursor:pointer}.tlki-slider:focus{outline:1px solid #8ed7ff}.tlki-slider-rail{position:absolute;left:0;right:0;top:13px;height:6px;border-radius:3px;background:#101a23}.tlki-slider-fill{height:6px;border-radius:3px;background:#51b6ed}.tlki-slider-zero{position:absolute;left:50%;top:-4px;width:1px;height:14px;background:#a3b7c7}.tlki-slider-thumb{position:absolute;top:-5px;width:16px;height:16px;margin-left:-8px;border:2px solid #e1f5ff;border-radius:8px;background:#389ed9}.tlki-control-ends{display:flex;justify-content:space-between;margin:5px 40px 0;font-size:10px;color:#b4c6d4}.tlki-control-help{font-size:10px;line-height:1.5;color:#aebfcd}.tlki-panel{background:#151d25}.tlki-status{padding:8px 10px;background:#243341;border-radius:6px}";
  css.textContent += ".tlki-control-card{display:flex;align-items:center;padding:9px 8px;margin-bottom:7px}.tlki-control-head{width:88px;flex-shrink:0;margin-bottom:0}.tlki-control-title{font-size:11px;margin-left:5px}.tlki-control-head>.tlki-control-icon{width:15px;height:15px}.tlki-control-row{flex:1;min-width:0;margin:0 8px}.tlki-control-value{width:47px;min-width:47px;font-size:11px;margin-right:7px}.tlki-control-reset,.tlki-nudge{width:23px;height:25px;padding:4px}.tlki-slider{min-width:30px;margin:0 8px}.tlki-filters{flex-wrap:wrap;padding:8px 14px}.tlki-filters .tlki-filter{flex:none;width:23%;margin:3px 1%;padding:8px 2px}.tlki-control-help{margin-top:7px}";
  css.textContent += ".tlki-filters{flex-wrap:nowrap;align-items:center;padding:2px 14px}.tlki-filter-label{width:44px;font-size:10px;flex-shrink:0}.tlki-filters .tlki-filter{flex:1;min-width:0;width:auto;margin:2px;padding:7px 2px}.tlki-placement-toggle{display:flex;align-items:center;width:90%;margin:10px 5%;padding:10px;border:1px solid #3b4c5b;border-radius:6px;background:#202c37;font-size:12px}.tlki-placement-toggle img{margin-right:8px}";
  var glyphs = {close:'Close',reset:'ArrowCircular',add:'Plus',auto:'ArrowCircular',trash:'Trash',arrow:'ArrowRight',info:'Info'};
  function icon(kind, color, size) {
    var img = document.createElement('span');
    img.style.display = 'inline-block'; img.style.flexShrink = '0';
    img.style.width = img.style.height = (size || 18) + 'px';
    img.style.backgroundColor = color || '#bdbec2';
    img.style.maskImage = 'url(Media/Glyphs/' + glyphs[kind] + '.svg)';
    img.style.maskSize = 'contain'; img.style.maskRepeat = 'no-repeat';
    img.setAttribute('aria-hidden', 'true'); return img;
  }
  function element(tag, cls, text, parent) { var e = document.createElement(tag); e.className = cls; e.style.fontFamily = '"Noto Sans KR"'; e.style.color = '#eeeeef'; if (text) { e.textContent = text; } if (parent) { parent.appendChild(e); } return e; }
  function pointer(e) { if(sliderDrag){sliderDrag.move(e);e.preventDefault();return;} if(drag&&panel){position=[e.clientX-drag[0],e.clientY-drag[1]];place();e.preventDefault();return;} if (!panel) { x = e.clientX; y = e.clientY; } }
  function stopDrag(){if(sliderDrag){var pending=sliderDrag;sliderDrag=null;pending.commit();}drag=null;if(panel){panel.style.opacity='1';panel.style.transform='scale(1)';}}
  document.addEventListener('mouseup',stopDrag);window.addEventListener('blur',stopDrag);
  document.addEventListener('mousemove', pointer);
  function close() { sliderDrag=null;drag=null;activeList=null;if (panel) { panel.remove(); panel = null; } }
  function clickSound() { engine.trigger('audio.playSound', 'select-item', 1); }
  function send(value) { clickSound(); engine.trigger('tlkIndividual.choose', value); }
  function bind(button, value) { button.type = 'button'; button.addEventListener('click', function (e) { e.stopPropagation(); send(value); }); }
  function action(parent, label, value, glyph, danger) { var b = element('button', 'tlki-action'+(danger?' tlki-danger':''), '', parent); b.appendChild(icon(glyph, danger?'#f19498':null)); element('span', '', label, b); bind(b, value); }
  function place() {if(!panel)return;if(!position)position=[x+18,y];var rect=panel.getBoundingClientRect();position[0]=Math.max(8,Math.min(position[0],window.innerWidth-rect.width-8));position[1]=Math.max(8,Math.min(position[1],window.innerHeight-rect.height-8));panel.style.left=position[0]+'px';panel.style.top=position[1]+'px';}
  var sliderDrag=null;
  function controlIcon(name,parent){var img=element('img','tlki-control-icon','',parent);img.src='coui://tlk/Icons/'+name+'.svg';img.alt='';return img;}
  function transformControl(parent,model,key,title,unit,min,max,step,command,glyph,left,right){
    var card=element('div','tlki-control-card','',parent),head=element('div','tlki-control-head','',card);
    controlIcon(glyph,head);element('span','tlki-control-title',title,head);
    var readout=element('span','tlki-control-value',''),value=Number(model[key])||0;
    var reset=element('button','tlki-control-reset','');reset.type='button';reset.title=title+' 초기화';reset.setAttribute('aria-label',reset.title);controlIcon('Reset',reset);
    var row=element('div','tlki-control-row','',card);
    function nudge(iconName,amount,label){var b=element('button','tlki-nudge','',row);b.type='button';b.title=label;b.setAttribute('aria-label',label);controlIcon(iconName,b);b.addEventListener('click',function(e){e.stopPropagation();set(value+amount);commit();});return b;}
    nudge('Minus',-step,'한 단계 줄이기');
    var track=element('div','tlki-slider','',row);track.tabIndex=0;track.setAttribute('role','slider');track.setAttribute('aria-label',title);track.setAttribute('aria-valuemin',String(min));track.setAttribute('aria-valuemax',String(max));
    var rail=element('div','tlki-slider-rail','',track),fill=element('div','tlki-slider-fill','',rail);element('div','tlki-slider-zero','',rail);var thumb=element('div','tlki-slider-thumb','',rail);
    nudge('Plus',step,'한 단계 늘리기');
    function set(next){value=Math.max(min,Math.min(max,Math.round(next/step)*step));var text=value.toFixed(step<1?1:0);readout.textContent=text+' '+unit;track.setAttribute('aria-valuenow',text);track.setAttribute('aria-valuetext',text+' '+unit);var percent=100*(value-min)/(max-min);fill.style.width=percent+'%';thumb.style.left=percent+'%';}
    function commit(){model[key]=value;send(command+':'+value.toFixed(step<1?1:0));}
    function move(e){var r=track.getBoundingClientRect();if(r.width>0)set(min+(max-min)*Math.max(0,Math.min(1,(e.clientX-r.left)/r.width)));}
    track.addEventListener('mousedown',function(e){if(e.button!==0)return;e.preventDefault();e.stopPropagation();move(e);sliderDrag={move:move,commit:commit};});
    track.addEventListener('keydown',function(e){var n=e.key==='ArrowLeft'||e.key==='ArrowDown'?value-step:e.key==='ArrowRight'||e.key==='ArrowUp'?value+step:e.key==='Home'?min:e.key==='End'?max:null;if(n===null)return;e.preventDefault();e.stopPropagation();set(n);commit();});
    reset.addEventListener('click',function(e){e.stopPropagation();set(0);commit();});
    card.appendChild(readout);card.appendChild(reset);track.title=left+' / '+right;set(value);
  }

  function assetLabel(name){return name.replace(/^CSKR/i,'').replace(/TrafficLight/gi,'').replace(/([a-z0-9])([A-Z])/g,'$1 $2').replace(/[_ ]+/g,' ').trim() || name;}
  function signKind(name){return name.indexOf('CSKRTrafficLightSignSpeed')===0?1:name.indexOf('CSKRTrafficLightSignBan')===0?3:2;}
  function signIcon(value) {
    var key=value.replace('CSKRTrafficLightSign','');
    var img=document.createElement('img');img.alt='';img.setAttribute('aria-hidden','true');img.style.width='26px';img.style.height='26px';img.style.flexShrink='0';img.style.marginRight='6px';
    img.src='coui://tlk/Icons/'+key+'.svg';return img;
  }
  function signLabel(name){var suffix=name.replace('CSKRTrafficLightSign','');return {StraightLeft:'직좌 동시',StraightThenStraightLeft:'직진 → 직좌',StraightLeftThenStraight:'직좌 → 직진',BanLeft:'좌회전 금지',BanRight:'우회전 금지',BanStraight:'직진 금지',BanUTurn:'유턴 금지'}[suffix]||suffix.replace('Speed','')+' km/h';}
  window.addEventListener('resize',place);
  window.tlkIndividual = {

    close: close,
    dispose: function () { close(); css.remove(); window.removeEventListener('resize',place); document.removeEventListener('mousemove', pointer); document.removeEventListener('mouseup',stopDrag);window.removeEventListener('blur',stopDrag);delete window.tlkIndividual; },
    show: function (model) {
      var previous=panel?{list:activeList?activeList.scrollTop:0,panel:panel.scrollTop}:null;
      close(); panel = element('div', 'tlki-panel'); panel.setAttribute('role', 'dialog'); panel.setAttribute('aria-label', model.title);
      panel.style.left = Math.max(8, Math.min(x + 18, window.innerWidth - 442)) + 'px'; panel.style.top = Math.max(8, y) + 'px';
      var header = element('div', 'tlki-head', '', panel), heading = element('div', 'tlki-heading', '');
      element('div', 'tlki-eyebrow', 'TRAFFIC LIGHT INTO KOREA', heading); element('div', 'tlki-title', model.title, heading);
      var grip=document.createElement('img');grip.className='tlki-grip';grip.alt='';grip.setAttribute('aria-hidden','true');
      grip.src='coui://tlk/Icons/Grip.svg';header.appendChild(grip);header.appendChild(heading);
      header.addEventListener('mousedown',function(e){if(e.button!==0)return;var target=e.target;while(target&&target!==header){if(target.tagName==='BUTTON'||target.tag==='button')return;target=target.parentNode;}place();drag=[e.clientX-position[0],e.clientY-position[1]];panel.style.transformOrigin='0 0';panel.style.opacity='.76';panel.style.transform='scale(.96)';e.preventDefault();e.stopPropagation();});
      var exit = element('button', 'tlki-close', '', header); exit.title = '닫기'; exit.setAttribute('aria-label', '닫기'); exit.appendChild(icon('close','#ff989e',18)); bind(exit, 'close');
      if (model.status) { element('div', 'tlki-status', model.status, panel); }
      var signalPanel=element('div',''),signPanel=element('div',''),propsPanel=element('div','');
      var tabs=element('div','tlki-tabs','',panel);tabs.setAttribute('role','tablist');tabs.setAttribute('aria-label','신호등 편집 항목');var tabButtons=['신호등','표지판','기타 요소'].map(function(label){return element('button','tlki-tab',label,tabs);});
      function selectTab(index){activeTab=index;[signalPanel,signPanel,propsPanel].forEach(function(part,i){var selected=i===index;part.style.display=selected?'block':'none';part.setAttribute('role','tabpanel');part.setAttribute('id','tlki-tab-panel-'+i);part.setAttribute('aria-labelledby','tlki-tab-'+i);tabButtons[i].className='tlki-tab'+(selected?' tlki-tab-active':'');tabButtons[i].style.color=selected?'#86ceff':'#a6b2bf';tabButtons[i].setAttribute('role','tab');tabButtons[i].setAttribute('id','tlki-tab-'+i);tabButtons[i].setAttribute('aria-controls','tlki-tab-panel-'+i);tabButtons[i].setAttribute('aria-selected',selected?'true':'false');});place();}
      tabButtons.forEach(function(button,i){button.type='button';button.addEventListener('click',function(e){e.stopPropagation();clickSound();selectTab(i);});});
      panel.appendChild(signalPanel);panel.appendChild(signPanel);panel.appendChild(propsPanel);var assets = (model.assets || []).filter(function(item){var name=typeof item==='string'?item:item.name;var combined=/crosswalk/i.test(name),left=/Left01$/i.test(name);return (lampFilter==='all'||new RegExp(lampFilter,'i').test(name))&&(poleFilter==='all'||(poleFilter==='left'?left:!left))&&(assetFilter==='all'||(assetFilter==='combined'?combined:!combined));});
      function filterGroup(label,group,selected,options){var filters=element('div','tlki-filters','',signalPanel);filters.setAttribute('role','group');filters.setAttribute('aria-label',label);element('span','tlki-filter-label',label,filters);options.forEach(function(entry){var button=element('button','tlki-filter'+(selected===entry[0]?' tlki-filter-active':''),entry[1],filters);button.type='button';button.setAttribute('aria-pressed',selected===entry[0]?'true':'false');button.addEventListener('click',function(e){e.stopPropagation();if(selected===entry[0])return;clickSound();if(group==='lamp')lampFilter=entry[0];else if(group==='pole')poleFilter=entry[0];else assetFilter=entry[0];if(activeList)activeList.scrollTop=0;window.tlkIndividual.show(model);});});}
      filterGroup('구 수','lamp',lampFilter,[['all','전체'],['3w','3구'],['4w','4구']]);
      filterGroup('기둥','pole',poleFilter,[['all','전체'],['left','좌 기둥'],['right','우 기둥']]);
      filterGroup('보행등','type',assetFilter,[['all','전체'],['signal','신호등만'],['combined','보행자 겸용']]);
      if(model.currentAsset){element('div','tlki-current-summary','현재 표시 · '+assetLabel(model.currentAsset),signalPanel);}
      var list = element('div', 'tlki-list', '', signalPanel), selectedRow = null;
      activeList=list;
      var gridLine;
      assets.forEach(function (item,index) {
        if(index%3===0){gridLine=element('div','tlki-grid-line','',list);}
        var name = typeof item === 'string' ? item : item.name, thumbnail = typeof item === 'string' ? '' : item.thumbnail;
        var selected = name === model.currentAsset;
        var row = element('button', 'tlki-row'+(selected?' tlki-selected':''), '', gridLine); row.title = name; row.setAttribute('aria-pressed',selected?'true':'false'); row.setAttribute('aria-label',assetLabel(name)+(selected?' · 현재 표시':'')); if(selected){selectedRow=row;}
        var tile = element('span', 'tlki-thumb', '', row), img = document.createElement('img');
        img.addEventListener('load',function(){var w=img.naturalWidth,h=img.naturalHeight;if(w>0&&h>0){var scale=100/Math.max(w,h);img.style.width=(w*scale)+'px';img.style.height=(h*scale)+'px';}});
        img.alt = ''; img.src = thumbnail || model.placeholder || 'Media/Placeholder.svg';
        img.addEventListener('error', function () { if (img.fallbackUsed) { img.style.visibility = 'hidden'; return; } img.fallbackUsed = true; img.src = model.placeholder || 'Media/Placeholder.svg'; });
        tile.appendChild(img); element('span', 'tlki-name', assetLabel(name), row); if(selected){var badge=element('span','tlki-current','',row);badge.style.display='flex';badge.style.alignItems='center';var check=document.createElement('img');check.alt='';check.setAttribute('aria-hidden','true');check.style.width='12px';check.style.height='12px';check.style.marginRight='3px';check.style.flexShrink='0';check.src='coui://tlk/Icons/Current.svg';badge.appendChild(check);element('span','','현재 표시',badge);} bind(row, 'asset:' + name);
      });
      if (!assets.length) { element('div', 'tlki-empty', '이 분류에 표시할 에셋이 없습니다. 모드 옵션에서 에셋을 추가하세요.', list); }
      if(model.far){var fold=element('button','tlki-placement-toggle','',signalPanel);fold.type='button';var foldIcon=element('img','tlki-control-icon','',fold);foldIcon.src='coui://tlk/Icons/Chevron.svg';foldIcon.alt='';element('span','','세부 배치',fold);var controls=element('div','tlki-transform-controls','',signalPanel);function foldState(){fold.setAttribute('aria-expanded',placementOpen?'true':'false');controls.style.display=placementOpen?'block':'none';foldIcon.style.transform=placementOpen?'rotate(90deg)':'rotate(0deg)';}fold.addEventListener('click',function(e){e.stopPropagation();placementOpen=!placementOpen;foldState();place();});foldState();
        transformControl(controls,model,'farOffset','앞뒤 위치','m',-3,3,.1,'offset','Move','원본 쪽으로','원본에서 멀리');
        transformControl(controls,model,'farLateral','좌우 위치','m',-3,3,.1,'lateral','Lateral','왼쪽으로','오른쪽으로');
        transformControl(controls,model,'farRotation','회전 각도','도',-45,45,1,'rotation','Rotate','왼쪽 회전','오른쪽 회전');
        element('div','tlki-control-help','드래그 후 놓으면 적용됩니다. 양옆 버튼으로 미세 조절할 수 있습니다.',controls);
      }
      var actions = element('div', 'tlki-actions', '', signalPanel);
      action(actions, '에셋 자동 선택으로 되돌리기', 'reset', 'reset');
      if (model.far) { if(model.nearHidden)action(actions,'본 위치 신호등 살리기','restoreNear','reset');action(actions, '이 맞은편 신호등만 삭제', 'delete', 'trash', true); }
      else { if(model.farReady)action(actions,'본 위치 신호등만 숨기기','hideNear','trash');else element('div','tlki-control-help','맞은편 신호등을 생성한 뒤 본 위치 신호등만 숨길 수 있습니다.',actions);action(actions, '맞은편 신호등 추가 요청', 'add', 'add'); action(actions, '맞은편 생성 여부를 자동으로 되돌리기', 'auto', 'auto'); }
      [1,2,3].forEach(function(kind){element('div','tlki-section',['속도 제한','신호 체계','금지 표지판'][kind-1],signPanel);if(!model.mounts||!model.mounts[kind-1]){element('div','tlki-empty','이 에셋에는 해당 표지판 부착 위치가 없습니다.',signPanel);return;}var group=element('div','tlki-sign-group', '',signPanel);group.setAttribute('role','radiogroup');group.setAttribute('aria-label',['속도 제한','신호 체계','금지 표지판'][kind-1]);group.style.display='flex';group.style.flexWrap='wrap';group.style.padding='0 18px 10px';var selected=(model.selections||[])[kind-1]||'auto';function option(value,label){var row=element('button','tlki-sign-choice','',group);row.type='button';row.style.width='31.33333%';row.style.margin='3px 1%';row.style.flexShrink='0';row.style.padding='9px 7px';row.style.display='flex';row.style.alignItems='center';row.style.minHeight='54px';row.style.borderRadius='6px';row.style.border='1px solid '+(selected===value?'rgba(125,199,237,.9)':'rgba(255,255,255,.17)');row.style.backgroundColor=selected===value?'rgba(60,142,187,.35)':'rgba(255,255,255,.035)';row.style.fontSize='11px';row.style.textAlign='left';row.style.whiteSpace='normal';row.setAttribute('role','radio');row.setAttribute('aria-checked',selected===value?'true':'false');row.appendChild(signIcon(value));var labelNode=element('span','',label,row);labelNode.style.minWidth='0';labelNode.style.lineHeight='1.35';labelNode.style.wordBreak='keep-all';row.addEventListener('click',function(e){e.stopPropagation();send('sign:'+kind+':'+value);});}option('auto','자동');option('none','표시 안함');(model.signs||[]).filter(function(n){return signKind(n)===kind;}).forEach(function(n){option(n,signLabel(n));});});
      element('div','tlki-note','유형별 하나만 선택됩니다. 수동 표지판은 통행 규칙을 바꾸지 않습니다. 금지 자동 선택 우선순위: 직진 · 좌회전 · 우회전 · 유턴.',signPanel);
      if((model.roadNames||[]).length){
        element('div','tlki-section','도로명 표지판',propsPanel);
        var roadGroup=element('div','tlki-sign-group','',propsPanel);roadGroup.style.display='flex';roadGroup.style.padding='8px 18px';roadGroup.setAttribute('role','radiogroup');roadGroup.setAttribute('aria-label','도로명 표지판 종류');
        (model.roadNames||[]).forEach(function(item){var chosen=model.roadName===item.name;var button=element('button','tlki-sign-choice','',roadGroup);button.type='button';button.style.width='31.33333%';button.style.margin='0 1%';button.style.padding='8px';button.style.border='1px solid '+(chosen?'#75cafa':'#56616d');button.style.background=chosen?'#254d65':'#202a32';button.style.borderRadius='6px';button.setAttribute('role','radio');button.setAttribute('aria-checked',chosen?'true':'false');button.title=item.name;var thumb=element('img','','',button);thumb.src=item.thumbnail||model.placeholder;thumb.alt='';thumb.style.width='64px';thumb.style.height='52px';thumb.style.objectFit='contain';thumb.addEventListener('error',function(){if(thumb.fallbackUsed)return;thumb.fallbackUsed=true;thumb.src=model.placeholder;});element('div','',item.name==='CSKRRoadNameSignBoth'?'양쪽':item.name==='CSKRRoadNameSignLeft'?'왼쪽':'오른쪽',button);button.addEventListener('click',function(e){e.stopPropagation();send('roadname:'+item.name);});});
        var roadReset=element('button','tlki-sign-choice','원래 구성으로 되돌리기',propsPanel);roadReset.type='button';roadReset.style.margin='4px 18px 12px';roadReset.setAttribute('aria-pressed',model.roadNameOriginal?'true':'false');roadReset.addEventListener('click',function(e){e.stopPropagation();send('roadname:');});
      }
      var propsGrid=element('div','tlki-props-grid','',propsPanel);propsGrid.style.display='flex';propsGrid.style.flexWrap='wrap';propsGrid.style.padding='12px 18px';
      (model.props||[]).forEach(function(item){
        var button=element('button','tlki-prop','',propsGrid);button.type='button';button.disabled=!!item.pending;button.title=item.name+' · '+(item.pending?'적용 중':item.enabled?'켜짐 · 클릭하여 끄기':'꺼짐 · 클릭하여 켜기');button.setAttribute('aria-label',button.title);button.setAttribute('aria-pressed',item.enabled?'true':'false');button.setAttribute('aria-busy',item.pending?'true':'false');
        button.style.position='relative';button.style.width='31.33333%';button.style.margin='4px 1%';button.style.padding='12px';button.style.height='114px';button.style.borderRadius='9px';button.style.border='1px solid '+(item.enabled?'#67ce91':'#6c7781');button.style.background=item.enabled?'rgba(53,132,87,.17)':'rgba(255,255,255,.035)';button.style.opacity=item.pending?'.55':'1';
        var thumb=document.createElement('img');thumb.alt='';thumb.src=item.thumbnail||model.placeholder||'Media/Placeholder.svg';thumb.style.width='80px';thumb.style.height='80px';thumb.style.objectFit='contain';thumb.style.opacity=item.enabled?'1':'.4';thumb.addEventListener('error',function(){if(thumb.fallbackUsed)return;thumb.fallbackUsed=true;thumb.src=model.placeholder||'Media/Placeholder.svg';});button.appendChild(thumb);
        var state=document.createElement('img');state.alt='';state.setAttribute('aria-hidden','true');state.src='coui://tlk/Icons/'+(item.enabled?'PropOn':'PropOff')+'.svg';state.style.position='absolute';state.style.top='5px';state.style.right='5px';state.style.width='20px';state.style.height='20px';button.appendChild(state);
        button.addEventListener('click',function(e){e.stopPropagation();if(button.disabled)return;button.disabled=true;button.setAttribute('aria-busy','true');send('prop:'+item.key+':'+(item.enabled?'0':'1'));});
      });
      if(!(model.props||[]).length)element('div','tlki-empty','이 에셋에 설정된 기타 요소가 없습니다.',propsPanel);
      element('div','tlki-note','미리 설정된 요소만 켜고 끌 수 있습니다. 초록색은 켜짐, 빈 회색은 꺼짐입니다.',propsPanel);
      selectTab(model.propsTab?2:model.signTab?1:previous?activeTab:0);
      var note = element('div', 'tlki-note', '', panel); note.appendChild(icon('info','#939499',14)); element('span', '', '도시 저장 시 개별 설정도 저장됩니다. 맞은편 추가는 안전한 배치 위치가 있을 때 적용됩니다.', note);
      document.body.appendChild(panel);
      if(previous){list.scrollTop=previous.list;panel.scrollTop=previous.panel;}else if(selectedRow){list.scrollTop=Math.max(0,selectedRow.parentNode.offsetTop-list.offsetTop-4);}
      if (window.getComputedStyle) { engine.trigger('tlkIndividual.diagnostic', 'font=' + window.getComputedStyle(panel).fontFamily + '; locale=' + document.documentElement.className + '; assets=' + assets.length); }
      var height = panel.getBoundingClientRect ? panel.getBoundingClientRect().height : 480;
      panel.style.top = Math.max(8, Math.min(y, window.innerHeight - height - 8)) + 'px';place();
    }
  };
}());
