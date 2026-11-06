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
