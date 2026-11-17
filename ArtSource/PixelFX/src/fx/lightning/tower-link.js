import { fit, rng } from '../../core/index.js';
import { VOLT, zigzag, stroke } from './_lib.js';
export const meta={name:'Tower link',size:[128,32],origin:[0,16],duration:0.5,loop:true,fps:12,bg:'none',options:{palette:['#7a3800','#b86a00','#efa800','#ffd21c','#ffe54c','#fff08a','#fffbd6','#ffffff'],seed:3}};
export function draw(p,t,o){const r=rng(o.seed,Math.floor(t*12));stroke(p,zigzag(r,0,0,127,0,0.075,0,1.2),1,fit(o.palette,8),1.5,{light:0.18,core:0.9});}
