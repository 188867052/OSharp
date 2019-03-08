import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ModuleComponent } from './module/module.component';
import { FunctionComponent } from './function/function.component';
import { RoleFunctionComponent } from './role-function/role-function.component';
import { UserFunctionComponent } from './user-function/user-function.component';
import { EntityinfoComponent } from './entityinfo/entityinfo.component';
import { RoleEntityinfoComponent } from './role-entityinfo/role-entityinfo.component';

const routes: Routes = [
  { path: 'module', component: ModuleComponent, data: { title: '模块信息管理', reuse: true } },
  { path: 'function', component: FunctionComponent, data: { title: '功能信息管理', reuse: true } },
  { path: 'role-function', component: RoleFunctionComponent, data: { title: '角色功能管理', reuse: true } },
  { path: 'user-function', component: UserFunctionComponent, data: { title: '用户功能管理', reuse: true } },
  { path: 'entityinfo', component: EntityinfoComponent, data: { title: '实体管理', reuse: true } },
  { path: 'role-entity', component: RoleEntityinfoComponent, data: { title: '角色数据管理', reuse: true } },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SecurityRoutingModule { }
