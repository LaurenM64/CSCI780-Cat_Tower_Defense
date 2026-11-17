import { hex,TAU } from '../../core/index.js';
import { drawLeaf } from './_lib.js';
export const meta={name:'Magical Leaf',size:[48,48],origin:[24,24],duration:1,loop:true,fps:12,bg:'none',options:{}};
const C=['#123c17','#277d2d','#73d747','#ddff96'].map(hex);
export function draw(p,t,o){p.glow(0,0,17,C[2],0.18);drawLeaf(p,0,0,23,11,t*TAU,0.85+0.15*Math.cos(t*TAU),C);}
