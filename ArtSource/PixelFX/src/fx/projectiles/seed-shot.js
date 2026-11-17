import { hex } from '../../core/index.js';
export const meta={name:'Bullet Seed',size:[32,32],origin:[16,16],duration:0.5,loop:true,fps:12,bg:'none',options:{}};
export function draw(p,t,o){p.glow(0,0,9,hex('#8aff48'),0.25);p.ellipse(0,0,5,3,hex('#276d22'));p.ellipse(0,-0.5,4,2,hex('#8bdf42'));p.line(-2,-1,2,-1,hex('#e8ffad'));p.px(3,0,hex('#bded66'));}
