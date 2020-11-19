declare namespace puerts {
    // import {$Ref, $Task, System} from "csharp"
    
    function $ref<T>(x? : T) : CS.$Ref<T>;
    
    function $unref<T>(x: CS.$Ref<T>) : T;
    
    function $set<T>(x: CS.$Ref<T>, val:T) : void;

    function $promise<T>(x: CS.$Task<T>) : Promise<T>;
    
    function $generic<T extends new (...args:any[]) => any> (genericType :T, ...genericArguments: (new (...args:any[]) => any)[]) : T;
    
    function $typeof(x : new (...args:any[]) => any) : CS.System.Type;
    
    function $extension(c : Function, e: Function) : void;
}

declare function require(name: string): any;