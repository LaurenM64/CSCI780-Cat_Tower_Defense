import { fit, rng } from '../../core/index.js';
import { VOLT, zigzag, stroke } from './_lib.js';
export const meta={name:'Tower link',size:[128,32],origin:[0,16],duration:0.5,loop:true,fps:12,bg:'none',options:{palette:VOLT,seed:3}};
export function draw(p,t,o){const r=rng(o.seed,Math.floor(t*12));stroke(p,zigzag(r,0,0,127,0,0.10,0,0.8),1,fit(o.palette,8),0.55,{light:0});}
