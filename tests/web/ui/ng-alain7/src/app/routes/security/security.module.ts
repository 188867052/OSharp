import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared';
import { SecurityRoutingModule } from './security.routing';
import { ModuleComponent } from './module/module.component';
import { FunctionComponent } from './function/function.component';
import { EntityinfoComponent } from './entityinfo/entityinfo.component';
import { RoleFunctionComponent } from './role-function/role-function.component';
import { UserFunctionComponent } from './user-function/user-function.component';
import { RoleEntityinfoComponent } from './role-entityinfo/role-entityinfo.component';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    SecurityRoutingModule
  ],
  declarations: [
    ModuleComponent,
    FunctionComponent,
    EntityinfoComponent,
    RoleFunctionComponent,
    UserFunctionComponent,
    RoleEntityinfoComponent
  ]
})
export class SecurityModule { }
